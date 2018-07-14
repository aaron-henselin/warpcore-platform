using System.Text;
using System.Web.DynamicData;
using System.Web.Routing;
using WarpCore.Web.ServiceModel;

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

            RouteTable.Routes.Add("PageDesignerApi",new Route("wc-api/pagedesigner/{*pathInfo}",new PageDesignerApiRouteHandler()));
            RouteTable.Routes.Add("Configurator", new Route("wc-api/configurator", new ConfiguratorRouteHandler()));

        }



    }
}
