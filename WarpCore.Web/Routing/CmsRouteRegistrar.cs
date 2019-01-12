using System.Web.Routing;
using WarpCore.Web.ServiceModel;

namespace WarpCore.Cms
{


    public class CmsRouteRegistrar
    {
        public static void RegisterDynamicRoutes()
        {
            RouteTable.Routes.RouteExistingFiles = true;

            RouteTable.Routes.Add("PageDesignerApi",new Route("wc-api/pagedesigner/{*pathInfo}",new PageDesignerApiRouteHandler()));
            RouteTable.Routes.Add("Configurator", new Route("wc-api/configurator", new ConfiguratorRouteHandler()));

            //RouteTable.Routes.Add("BlazorModules", new Route("{*pathInfo}", new RouteValueDictionary(), new RouteValueDictionary(), new BlazorHostRouteHandler()));
            RouteTable.Routes.Add("BlazorModules", new Route("{folder}/{*resource}",new RouteValueDictionary(), new RouteValueDictionary(new{ folder="_framework"}), new BlazorHostRouteHandler()));
        }



    }
}
