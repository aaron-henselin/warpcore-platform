using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Routing;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Web.RenderingEngines.Mvc;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc
{
    public class MvcFragmentRenderer : IFragmentRenderer
    {
        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {
            var collection = new RenderingFragmentCollection();
            var allDescendents = pp.GetAllDescendents().OfType<ControllerPageCompositionElement>();
            foreach (var item in allDescendents)
            {
                //todo: cache.
                var descriptor = new ReflectedAsyncControllerDescriptor(item.Controller.GetType());

                var wrapper = new HttpContextWrapper(HttpContext.Current);
                var rq = new RequestContext
                {
                    HttpContext = wrapper
                };
                rq.RouteData = new RouteData();
                rq.RouteData.Values.Add("controller", descriptor.ControllerName);
                rq.RouteData.Values.Add("action","Index");

                

               item.Controller.Execute(rq);


                throw new Exception();
            }
            return collection;
        }
    }
}