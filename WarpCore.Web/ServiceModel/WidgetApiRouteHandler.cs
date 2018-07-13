using System;
using System.Web;
using System.Web.Routing;
using WarpCore.Web.ServiceModel;

namespace CatalystFire.CmsKit.Web.Framework.WidgetApi
{

    public class PageDesignerApiRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new PageDesignerApiHttpHandler();
        }
    }
}