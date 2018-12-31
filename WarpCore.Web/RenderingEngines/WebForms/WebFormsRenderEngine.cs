using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets;

namespace WarpCore.Web.RenderingEngines.WebForms
{
    public class WebFormsRenderEngine : IBatchingRenderEngine
    {
        private class SwitchingHtmlWriter : StringWriter
        {
            private readonly Stack<Guid> _idStack = new Stack<Guid>();

            public readonly Dictionary<Guid,List<ITransformOutput>> output = new Dictionary<Guid, List<ITransformOutput>>();

            

            public void BeginWriting(Guid id)
            {
                if (_idStack.Count > 0)
                {
                    var sb = this.GetStringBuilder();
                    if (!output[_idStack.Peek()].Any())
                        output[_idStack.Peek()].Add(new BeginWidgetHtmlOutput(sb));
                    else
                        output[_idStack.Peek()].Add(new HtmlOutput(sb));

                    sb.Clear();
                }

                _idStack.Push(id);
                output.Add(id, new List<ITransformOutput>());

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
                output[id].Add(new EndWidgetHtmlOutput(sb));
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
            private readonly Guid _id;

            public RenderingEngineComponent(WebFormsWidget pp)
            {
                _id = pp.ContentId;

                var control = pp.GetControl();
                Controls.Add(control);

                foreach (var ph in pp.PlaceHolders.Values)
                {
                    var contentPlaceHolder = control.FindControl(ph.Id);
                    contentPlaceHolder.Controls.Add(new LayoutSubstitutionComponent(ph));
                }

            }

            protected override void Render(HtmlTextWriter writer)
            {
                
                var switching = (SwitchingHtmlWriter)writer.InnerWriter;
                switching.BeginWriting(_id);
                base.Render(writer);
                switching.EndWriting();
            }
        }

        private class NonWebFormsControl : Control
        {
          
        }

        private static void BuildServerSidePage(Control nativeRoot, PartialPageRendering pp)
        {
            foreach (var kvp in pp.PlaceHolders)
            {
                var placeHolder = kvp.Value;

                Control contentPlaceHolder= nativeRoot.FindControl(placeHolder.Id);

                if (contentPlaceHolder == null)
                    throw new Exception("Placeholder " + placeHolder.Id + " does not exist.");

                foreach (var placedRendering in placeHolder.Renderings)
                {
                    if (placedRendering is WebFormsWidget)
                    {
                        WebFormsWidget webFormsRendering = ((WebFormsWidget) placedRendering);
                        var control = webFormsRendering.GetControl();
                        contentPlaceHolder.Controls.Add(new RenderingEngineComponent((WebFormsWidget)placedRendering));

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

        public static class LayoutBuilderIds
        {
            public static Guid PageRoot = Guid.Empty;
            public static Guid WebFormsInterop = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        }


        public CompositableContent Execute(PartialPageRendering pp)
        {

            SwitchingHtmlWriter _writer = new SwitchingHtmlWriter();
            
            
            var isWebFormsInChargeOfPageBase = pp is WebFormsPageRendering;
            if (isWebFormsInChargeOfPageBase)
            {
                var nativePageRendering = (WebFormsPageRendering)pp;
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
                        throw new Exception("Add a <head runat=server> tag in order to use this master page as a layout.");

                    if (nativePage.Form == null)
                        throw new Exception("Add a <form runat=server> tag in order to use this master page as a layout.");


                    nativePage.Header.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.Head ));
                    nativePage.Form.Controls.Add(new GlobalSubstitionComponent(GlobalLayoutPlaceHolderIds.Scripts ));

                };
              
                _writer.BeginWriting(pp.ContentId);
                HttpContext.Current.Server.Execute(nativePage, _writer, true);
                _writer.EndWriting();
            }
            else
            {
                var nativeRoot = new Page();
                var body = new HtmlGenericControl("body");
                var form = new HtmlGenericControl("form");


                var wrapper = new RenderingEngineComponent(new WebFormsWidget(body,LayoutBuilderIds.WebFormsInterop));
                wrapper.Controls.Add(body);
                body.Controls.Add(form);
                nativeRoot.Controls.Add(wrapper);

                foreach (var placeholder in pp.PlaceHolders)
                    form.Controls.Add(new ContentPlaceHolder {ID=placeholder.Value.Id});

                BuildServerSidePage(nativeRoot,pp);
                HttpContext.Current.Server.Execute(nativeRoot, _writer, true);
            }

            
            

            return new CompositableContent
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