using System;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{

    public class WebFormsFragmentRenderer : IFragmentRenderer
    {

        private class GlobalSubstitionComponent : Control
        {
            private readonly string _id;


            public GlobalSubstitionComponent(string id)
            {
                _id = id;
            }

            protected override void Render(HtmlTextWriter writer)
            {

                var switching = writer.InnerWriter as SwitchingHtmlWriter;
                switching?.AddGlobalSubsitution(_id);
            }
        }

        [DebuggerDisplay("ClientID = {" + nameof(ClientID) + ("}, Subsitution = {" + nameof(_placeholderId) + "}"))]
        private class SubsitutionHtmlWriterDirective : Control
        {
            private readonly RenderingsPlaceHolder _ph;
            private string _placeholderId;

            public SubsitutionHtmlWriterDirective(RenderingsPlaceHolder ph)
            {
                _ph = ph;
                _placeholderId = ph.Id;
            }

            protected override void Render(HtmlTextWriter writer)
            {
                //this is necessary because a NullTextWriter is possible when WebForms internally opts to not render something
                var switching = writer.InnerWriter as SwitchingHtmlWriter;
                switching?.AddLayoutSubsitution(_ph.Id);
            }
        }

        [DebuggerDisplay("ClientID = {" + nameof(ClientID) + "}, Render = {" + nameof(RenderHtmlFor) + "}")]
        private class RenderHtmlWriterDirective : PlaceHolder
        {
            private readonly WebFormsControlPageCompositionElement _pp;
            private string RenderHtmlFor { get; set; }

            public RenderHtmlWriterDirective(WebFormsControlPageCompositionElement pp)
            {
                _pp = pp;


                
            }

            public void Initialize()
            {
                var control = _pp.GetControl();
                RenderHtmlFor = control.GetType().Name;
                Controls.Add(control);

                foreach (var ph in _pp.PlaceHolders.Values)
                {
                    var contentPlaceHolder = control.FindControl(ph.Id);
                    if (contentPlaceHolder == null)
                        throw new Exception(
                            $"Placeholder with id '{ph.Id}' cannot be found in the '{control.ID}' naming container.");

                    contentPlaceHolder.Controls.Add(new SubsitutionHtmlWriterDirective(ph));
                }
            }


            protected override void Render(HtmlTextWriter writer)
            {
                //this is necessary because a NullTextWriter is possible when WebForms internally opts to not render something
                var switching = writer.InnerWriter as SwitchingHtmlWriter;
                switching?.BeginWriting(_pp);
                base.Render(writer);
                switching?.EndWriting();
            }
        }

        [DebuggerDisplay("ClientID = {" + nameof(ClientID) + "}, (DoNotRender)")]
        private class DoNotRenderHtmlWriterDirective : Control, INamingContainer
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
                        WebFormsControlPageCompositionElement webFormsCompositionElement = ((WebFormsControlPageCompositionElement)placedRendering);
                        AddRenderHtmlDirective(webFormsCompositionElement, contentPlaceHolder);

                        var control = webFormsCompositionElement.GetControl();
                        BuildServerSidePage(control, placedRendering);
                    }
                    else
                    {
                        var directive = AddDoNotRenderDirective(placedRendering, contentPlaceHolder);
                        BuildServerSidePage(directive, placedRendering);
                    }
                }
            }
        }

        private static DoNotRenderHtmlWriterDirective AddDoNotRenderDirective(PageCompositionElement placedRendering, Control contentPlaceHolder)
        {
            var control = new DoNotRenderHtmlWriterDirective {ID = placedRendering.LocalId};
            foreach (var nonWebFormsPlaceHolder in placedRendering.PlaceHolders.Values)
                control.Controls.Add(new DoNotRenderHtmlWriterDirective {ID = nonWebFormsPlaceHolder.Id});

            contentPlaceHolder.Controls.Add(control);

            return control;
        }

        private static RenderHtmlWriterDirective AddRenderHtmlDirective(WebFormsControlPageCompositionElement placedRendering, Control contentPlaceHolder)
        {
            var compositionElement = (WebFormsControlPageCompositionElement) placedRendering;
            var wrapped = new RenderHtmlWriterDirective(compositionElement);
            contentPlaceHolder.Controls.Add(wrapped);
            wrapped.Initialize();
            return wrapped;
        }


        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {

            SwitchingHtmlWriter _writer = new SwitchingHtmlWriter();
            
            
            var isWebFormsInChargeOfPageBase = pp is WebFormsPageCompositionElement;
            if (isWebFormsInChargeOfPageBase)
            {
                var nativePageRendering = (WebFormsPageCompositionElement)pp;
                var nativePage = nativePageRendering.GetPage();
                nativePage.PreInit += (sender, args) =>
                {
                    var topMostControl = nativePage.GetRootControl();
                    foreach (var ph in pp.PlaceHolders.Values)
                    {
                        var contentPlaceHolder = topMostControl.FindControl(ph.Id);
                        contentPlaceHolder.Controls.Add(new SubsitutionHtmlWriterDirective(ph));
                    }

                    BuildServerSidePage(topMostControl, pp);
                };
                nativePage.Init += (sender, args) =>
                {

                };
                nativePage.InitComplete += (sender, args) =>
                {

                    if (nativePage.Header == null)
                        throw new Exception(
                            "Add a <head runat='server'> tag in order to use this master page as a layout.");

                    if (nativePage.Form == null)
                        throw new Exception(
                            "Add a <form runat='server'> tag in order to use this master page as a layout.");


                    nativePage.Header.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.Head));
                    nativePage.Form.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.Scripts));
                    nativePage.Form.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.InternalStateTracking));
                };

             
                _writer.BeginWriting(pp);


                var originalHandler = HttpContext.Current.Handler;
                try
                {
                    HttpContext.Current.Handler = nativePage; // required in order to process postbacks.
                                                              // if aspnet thinks that another renderer is in charge of the
                                                              // the execution, then 'GET' content is returned only.

                    HttpContext.Current.Server.Execute(nativePage, _writer, true);
                }
                finally
                {
                    HttpContext.Current.Handler = originalHandler;
                }
                
                _writer.EndWriting();
            }
            else
            {
                var nativeRoot = new System.Web.UI.Page();
                var body = new HtmlGenericControl("body");
                var form = new HtmlGenericControl("form");


                var wrapper = new RenderHtmlWriterDirective(new WebFormsControlPageCompositionElement(body){ContentId = SpecialRenderingFragmentContentIds.WebFormsInterop });
                wrapper.Controls.Add(body);
                wrapper.Initialize();

                body.Controls.Add(form);
                nativeRoot.Controls.Add(wrapper);

                foreach (var placeholder in pp.PlaceHolders)
                    form.Controls.Add(new ContentPlaceHolder {ID=placeholder.Value.Id});

                BuildServerSidePage(nativeRoot,pp);
                HttpContext.Current.Server.Execute(nativeRoot, _writer, true);
            }

            return new RenderingFragmentCollection
            {
                RenderingResults = _writer.output,
            };
        }

        private void NativePage_InitComplete(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}