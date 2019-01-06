using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Routing;
using System.Web.WebPages;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Web.RenderingEngines.Mvc;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc
{
    public class NoResponseHttpContextBase : HttpContextWrapper
    {
        
        private HttpResponseWrapper _response;

        public NoResponseHttpContextBase(HttpContext httpContext, StringWriter writer) : base(httpContext)
        {
            _response = new HttpResponseWrapper(new HttpResponse(writer));
        }

        public override HttpResponseBase Response => _response;

        
        
    }

    

    public static class MvcHelpers
    {
        public static MvcHtmlString PlaceHolder(this HtmlHelper htmlHelper,string placeHolderId)
        {
            var response = htmlHelper.ViewContext.RequestContext.HttpContext.Response;
            var sb = ((StringWriter)htmlHelper.ViewContext.Writer).GetStringBuilder();
            response.Write(sb);
            sb.Length = 0;

            var switching = (SwitchingHtmlWriter) response.Output;
            switching.AddLayoutSubsitution(placeHolderId);

            //var actual = htmlHelper.ViewContext.Writer;
            //actual.Flush();
            //var noReponse = (SwitchingHtmlWriter)htmlHelper.ViewContext.RequestContext.HttpContext.Response.Output;
            //noReponse.Write(actual.ToString());
            //noReponse.AddLayoutSubsitution(placeHolderId);
            //htmlHelper.ViewContext.Writer = new StringWriter();

            return MvcHtmlString.Empty;
        }
    }


    public class MvcFragmentRenderer : IFragmentRenderer
    {
        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {
            RenderingFragmentCollection collection = new RenderingFragmentCollection();
            
            var allDescendents = pp.GetAllDescendents().OfType<ControllerPageCompositionElement>();
            foreach (var item in allDescendents)
            {
                //todo: cache.
                var descriptor = new ReflectedAsyncControllerDescriptor(item.Controller.GetType());

                using (var writer = new SwitchingHtmlWriter())
                {
                    var fake = new NoResponseHttpContextBase(HttpContext.Current,writer);
                    var rq = new RequestContext
                    {
                        HttpContext = fake,
                    };
                    rq.RouteData = new RouteData();
                    rq.RouteData.Values.Add("controller", descriptor.ControllerName);
                    rq.RouteData.Values.Add("action", "Index");
                   
                    writer.BeginWriting(item);
                    item.Controller.Execute(rq);
                    writer.EndWriting();


                    collection.WidgetContent.Add(item.ContentId,writer.output[item.ContentId]);

                    //var placeholderOrder = new Dictionary<string, int>();
                    //foreach (var phId in item.PlaceHolders.Keys)
                    //    placeholderOrder.Add(phId,
                    //        output.IndexOf($"[##Placeholder({nonce+"-"+phId})]", StringComparison.OrdinalIgnoreCase));

                    //var phLocations = placeholderOrder.OrderBy(x => x.Value).Select(x => x.Key).ToList();

                    //List<IRenderingFragment> captured = new List<IRenderingFragment>();
                    //var allSegments = phLocations.Select(x => new{Id=x,Marker=$"[##Placeholder({nonce+"-"+x})]"}).ToList();

                    //var remainingOutput = output;
                    //foreach (var segment in allSegments)
                    //{
                    //    var at = remainingOutput.IndexOf(segment.Marker,StringComparison.OrdinalIgnoreCase);
                    //    if (at == -1)
                    //        throw new Exception("Placeholder with id '" + segment.Id+
                    //                            "' was defined in the internal layout, but was not rendered.");

                    //    var capturedHtml = remainingOutput.Substring(0, at);
                    //    if (capturedHtml.Length >0)
                    //        captured.Add(new HtmlOutput(capturedHtml));

                    //    captured.Add(new LayoutSubstitutionOutput{Id=segment.Id});

                    //    remainingOutput = remainingOutput.Substring(at + segment.Marker.Length);
                    //}
                    //if (remainingOutput.Length >0)
                    //    captured.Add(new HtmlOutput(remainingOutput));



                    //collection.WidgetContent.Add(item.ContentId,captured);
                }
            }
            return collection;
        }
    }
}