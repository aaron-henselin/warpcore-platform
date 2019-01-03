using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.Kernel;
using WarpCore.Web;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{

    public class WebFormsPartialPageRenderingFactory : IPartialPageRenderingFactory
    {
        public object ActivateType(Type type)
        {
            //allows us to pass in request context and such through to webforms controls.
            //I'm not sure that it really helps me though, because we're still tied to httpcontext for everything.
            return Dependency.Resolve(type);
        }

        public PageCompositionElement CreateRenderingForObject(object nativeWidgetObject)
        {
            return new WebFormsControlPageCompositionElement((Control) nativeWidgetObject);
        }

        public PageCompositionElement CreateRenderingForPhysicalFile(string physicalFilePath)
        {
            var vPath = "/App_Data/DynamicPage.aspx";
            Page page = BuildManager.CreateInstanceFromVirtualPath("/App_Data/DynamicPage.aspx", typeof(Page)) as Page;
            page.MasterPageFile = physicalFilePath;
            
            return new WebFormsPageCompositionElement(page){ContentId = SpecialRenderingFragmentContentIds.PageRoot};
        }

        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new []{typeof(Control)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new[] {KnownPhysicalFileExtensions.MasterPage};
        }
    }

    public class WebFormsRenderEngine : IBatchingRenderEngine
    {
        private class SwitchingHtmlWriter : StringWriter
        {
            private readonly Stack<Guid> _idStack = new Stack<Guid>();

            public readonly Dictionary<Guid,List<IRenderingFragment>> output = new Dictionary<Guid, List<IRenderingFragment>>();

            

            public void BeginWriting(PageCompositionElement pp)
            {
                if (pp.ContentId == Guid.Empty)
                    throw new ArgumentException($"Invalid page content id of '{pp.ContentId}' for {pp.FriendlyName}");

                var id = pp.ContentId;

                if (_idStack.Count > 0)
                {
                    var sb = this.GetStringBuilder();
                    if (!output[_idStack.Peek()].Any())
                        output[_idStack.Peek()].Add(new HtmlOutput(sb));

                    sb.Clear();
                }

                _idStack.Push(id);

                if (output.ContainsKey(id))
                    throw new Exception($"Output for CmsPageContent {id} has already been recorded.");

                output.Add(id, new List<IRenderingFragment>());

            }

            public void AddLayoutSubsitution(string id)
            {
                var sb = this.GetStringBuilder();
                output[_idStack.Peek()].Add(new HtmlOutput(sb));
                sb.Clear();

                output[_idStack.Peek()].Add(new LayoutSubstitutionOutput{Id = id});
            }

            public void AddGlobalSubsitution(string id)
            {
                var sb = this.GetStringBuilder();
                output[_idStack.Peek()].Add(new HtmlOutput(sb));
                sb.Clear();

                output[_idStack.Peek()].Add(new GlobalSubstitutionOutput { Id = id });
            }

            public void EndWriting()
            {
                var id = _idStack.Pop();

                var sb = this.GetStringBuilder();
                output[id].Add(new HtmlOutput(sb));
                sb.Clear();
              
            }
            

        }

        private class GlobalSubstitionComponent : Control
        {
            private readonly string _id;


            public GlobalSubstitionComponent(string id)
            {
                _id = id;
            }

            protected override void Render(HtmlTextWriter writer)
            {

                var switching = (SwitchingHtmlWriter)writer.InnerWriter;
                switching.AddGlobalSubsitution(_id);
            }
        }

        private class LayoutSubstitutionComponent : Control
        {
            private readonly RenderingsPlaceHolder _ph;

            public LayoutSubstitutionComponent(RenderingsPlaceHolder ph)
            {
                _ph = ph;
            }

            protected override void Render(HtmlTextWriter writer)
            {

                var switching = (SwitchingHtmlWriter)writer.InnerWriter;
                switching.AddLayoutSubsitution(_ph.Id);
            }
        }

        private class RenderingEngineComponent : PlaceHolder
        {
            private readonly WebFormsControlPageCompositionElement _pp;

            public RenderingEngineComponent(WebFormsControlPageCompositionElement pp)
            {
                _pp = pp;

                var control = pp.GetControl();
                Controls.Add(control);

                foreach (var ph in pp.PlaceHolders.Values)
                {
                    var contentPlaceHolder = control.FindControl(ph.Id);
                    if (contentPlaceHolder == null)
                        throw new Exception(
                            $"Placeholder with id '{ph.Id}' cannot be found in the '{control.ID}' naming container.");

                    contentPlaceHolder.Controls.Add(new LayoutSubstitutionComponent(ph));
                }

            }

            protected override void Render(HtmlTextWriter writer)
            {
                
                var switching = (SwitchingHtmlWriter)writer.InnerWriter;
                switching.BeginWriting(_pp);
                base.Render(writer);
                switching.EndWriting();
            }
        }

        private class NonWebFormsControl : Control
        {
          
        }

        private static void BuildServerSidePage(Control nativeRoot, PageCompositionElement pp)
        {
            foreach (var kvp in pp.PlaceHolders)
            {
                var placeHolder = kvp.Value;

                Control contentPlaceHolder= nativeRoot.FindControl(placeHolder.Id);

                if (contentPlaceHolder == null)
                    throw new Exception("Placeholder " + placeHolder.Id + " does not exist.");

                foreach (var placedRendering in placeHolder.Renderings)
                {
                    if (placedRendering is WebFormsControlPageCompositionElement)
                    {
                        WebFormsControlPageCompositionElement webFormsCompositionElement = ((WebFormsControlPageCompositionElement) placedRendering);
                        var control = webFormsCompositionElement.GetControl();

                        var compositionElement =(WebFormsControlPageCompositionElement) placedRendering;
                        var wrapped = new RenderingEngineComponent(compositionElement);
                        contentPlaceHolder.Controls.Add(wrapped);
                        //placedRendering.TryActivateLayout(compositionElement.GetControl() as ILayoutControl);


                        BuildServerSidePage(control, placedRendering);
                    }
                    else
                    {
                        var control = new NonWebFormsControl {ID=placedRendering.LocalId};
                        foreach (var nonWebFormsPlaceHolder in placedRendering.PlaceHolders.Values)
                            control.Controls.Add(new NonWebFormsControl {ID = nonWebFormsPlaceHolder.Id});

                        contentPlaceHolder.Controls.Add(control);
                        BuildServerSidePage(control, placedRendering);
                    }
                }
            }
        }




        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {

            SwitchingHtmlWriter _writer = new SwitchingHtmlWriter();
            
            
            var isWebFormsInChargeOfPageBase = pp is WebFormsPageCompositionElement;
            if (isWebFormsInChargeOfPageBase)
            {
                var nativePageRendering = (WebFormsPageCompositionElement)pp;
                var nativePage = nativePageRendering.GetPage();
                var topMostControl = nativePage.GetRootControl();
                
                //nativePage.Header.Controls.Add(new WebFormsWidget());

                foreach (var ph in pp.PlaceHolders.Values)
                {
                    var contentPlaceHolder = topMostControl.FindControl(ph.Id);
                    contentPlaceHolder.Controls.Add(new LayoutSubstitutionComponent(ph));
                }


                BuildServerSidePage(topMostControl, pp);

                //allows nonwebforms controls to get access to the head and the scripts
                nativePage.InitComplete += (sender, args) =>
                {
                    if (nativePage.Header == null)
                        throw new Exception("Add a <head runat='server'> tag in order to use this master page as a layout.");

                    if (nativePage.Form == null)
                        throw new Exception("Add a <form runat='server'> tag in order to use this master page as a layout.");


                    nativePage.Header.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.Head ));
                    nativePage.Form.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.Scripts ));
                    nativePage.Form.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.InternalStateTracking));

                };
              
                _writer.BeginWriting(pp);
                HttpContext.Current.Server.Execute(nativePage, _writer, true);
                _writer.EndWriting();
            }
            else
            {
                var nativeRoot = new Page();
                var body = new HtmlGenericControl("body");
                var form = new HtmlGenericControl("form");


                var wrapper = new RenderingEngineComponent(new WebFormsControlPageCompositionElement(body){ContentId = SpecialRenderingFragmentContentIds.WebFormsInterop });
                
                wrapper.Controls.Add(body);
                body.Controls.Add(form);
                nativeRoot.Controls.Add(wrapper);

                foreach (var placeholder in pp.PlaceHolders)
                    form.Controls.Add(new ContentPlaceHolder {ID=placeholder.Value.Id});

                BuildServerSidePage(nativeRoot,pp);
                HttpContext.Current.Server.Execute(nativeRoot, _writer, true);
            }

            
            

            return new RenderingFragmentCollection
            {
                WidgetContent = _writer.output,


            };
        }

        private void NativePage_InitComplete(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}