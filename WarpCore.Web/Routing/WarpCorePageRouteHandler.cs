using System.Web;
using System.Web.Routing;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public class WarpCorePageRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var requestUri = requestContext.HttpContext.Request.Url;

            var success = CmsRoutes.Current.TryResolveRoute(requestUri, out var route);
            if (success)
            {
                requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.RouteDataToken, route);

                if ("1" == requestContext.HttpContext.Request["wc_preview"])
                    requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.ContentEnvironmentToken, ContentEnvironment.Draft);
                else
                    requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.ContentEnvironmentToken, ContentEnvironment.Live);
            }
            else
            {
                throw new HttpException(404, "Page cannot be found.");
            }

            return new WarpCorePageHttpHandler();
        }
    }
}