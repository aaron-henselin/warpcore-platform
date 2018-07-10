using System.Text;
using System.Web.DynamicData;
using System.Web.Routing;

namespace WarpCore.Cms
{
    public struct CmsRouteDataTokens
    {
        public const string RouteDataToken = "WC_RTCONTEXT";
        public const string OriginalUriToken = "WC_ORIGINALURI";
    }

    public class CmsRouteRegistrar
    {
        public static void RegisterDynamicRoutes()
        {
            RouteTable.Routes.RouteExistingFiles = true;
            //RouteTable.Routes.Add("DynamicRoute",new Route("{*url}",new WarpCorePageRouteHandler()));
        }



    }
}
