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
    public class NoResponseHttpContextBase : HttpContextWrapper, IDisposable
    {
        private readonly StringBuilder sb;
        private StringWriter _stringWriter;
        private HttpResponseWrapper _response;

        public NoResponseHttpContextBase(HttpContext httpContext) : base(httpContext)
        {
            sb = new StringBuilder();
            _stringWriter = new StringWriter(sb);
            _response = new HttpResponseWrapper(new HttpResponse(_stringWriter));
          
        }

        public override HttpResponseBase Response => _response;

        public string GetWrittenOutput()
        {
            return _stringWriter.ToString();
        }

        public void Dispose()
        {
            
            _stringWriter?.Dispose();
        }
    }

    public static class MvcHelpers
    {
        public static MvcHtmlString PlaceHolder(this HtmlHelper htmlHelper,string placeHolderId)
        {
            var nonce = HttpContext.Current.Items["RenderingNonce"].ToString();
            return new MvcHtmlString($"[##Placeholder({nonce+"-"+placeHolderId})]");
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

                using (var fake = new NoResponseHttpContextBase(HttpContext.Current))
                {
                    var rq = new RequestContext
                    {
                        HttpContext = fake,
                    };
                    rq.RouteData = new RouteData();
                    rq.RouteData.Values.Add("controller", descriptor.ControllerName);
                    rq.RouteData.Values.Add("action", "Index");

                    var nonce = Guid.NewGuid()+"-"+item.ContentId;
                    rq.HttpContext.Items["RenderingNonce"] = nonce;

                    item.Controller.Execute(rq);

                    var output = fake.GetWrittenOutput();


                    var placeholderOrder = new Dictionary<string, int>();
                    foreach (var phId in item.PlaceHolders.Keys)
                        placeholderOrder.Add(phId,
                            output.IndexOf($"[##Placeholder({nonce+"-"+phId})]", StringComparison.OrdinalIgnoreCase));
                    
                    var phLocations = placeholderOrder.OrderBy(x => x.Value).Select(x => x.Key).ToList();

                    List<IRenderingFragment> captured = new List<IRenderingFragment>();
                    var allSegments = phLocations.Select(x => new{Id=x,Marker=$"[##Placeholder({nonce+"-"+x})]"}).ToList();

                    var remainingOutput = output;
                    foreach (var segment in allSegments)
                    {
                        var at = remainingOutput.IndexOf(segment.Marker,StringComparison.OrdinalIgnoreCase);
                        if (at == -1)
                            throw new Exception("Placeholder with id '" + segment.Id+
                                                "' was defined in the internal layout, but was not rendered.");

                        var capturedHtml = remainingOutput.Substring(0, at);
                        if (capturedHtml.Length >0)
                            captured.Add(new HtmlOutput(capturedHtml));

                        captured.Add(new LayoutSubstitutionOutput{Id=segment.Id});

                        remainingOutput = remainingOutput.Substring(at + segment.Marker.Length);
                    }
                    if (remainingOutput.Length >0)
                        captured.Add(new HtmlOutput(remainingOutput));


                   
                    collection.WidgetContent.Add(item.ContentId,captured);
                }
            }
            return collection;
        }
    }
}