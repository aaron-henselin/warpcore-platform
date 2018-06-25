using System.Text;
using System.Web.DynamicData;
using System.Web.Routing;

namespace WarpCore.Cms
{
    public struct CmsRouteDataTokens
    {
        public const string RouteDataToken = "WC_ROUTE";
        public const string ContentEnvironmentToken = "WC_CONTENTENV";
    }

    public class CmsRouteRegistrar
    {
        public static void RegisterDynamicRoutes()
        {
            RouteTable.Routes.RouteExistingFiles = true;
            RouteTable.Routes.Add("DynamicRoute",new Route("{*url}",new WarpCorePageRouteHandler()));
        }



    }
}
