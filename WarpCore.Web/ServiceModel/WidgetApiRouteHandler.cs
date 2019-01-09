using System.Web;
using System.Web.Routing;

namespace WarpCore.Web.ServiceModel
{
    public class BlazorHostRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {

            return new BlazorHostHttpHandler();
        }
    }

    public class ConfiguratorRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new ConfiguratorHttpHandler();
        }
    }

    public class PageDesignerApiRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new PageDesignerApiHttpHandler();
        }
    }
}