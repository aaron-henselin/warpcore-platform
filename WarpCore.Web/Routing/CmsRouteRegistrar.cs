using System;
using System.Web;
using System.Web.Routing;

namespace WarpCore.Cms
{


    public class CmsRouteRegistrar
    {
        public static void RegisterDynamicRoutes()
        {
            RouteTable.Routes.RouteExistingFiles = false;
            RouteTable.Routes.Ignore("{file}.js");

            //RouteTable.Routes.Add("BlazorModules", new Route("{*pathInfo}", new RouteValueDictionary(), new RouteValueDictionary(), new BlazorHostRouteHandler()));
            //var constraints = new RouteValueDictionary();
            //constraints.Add("url", new AppUrlConstraint());
            //RouteTable.Routes.Add("BlazorModules", new Route("{url}.js",defaults: new RouteValueDictionary(), constraints:constraints,routeHandler: new BlazorHostRouteHandler()));

 
        }



    }

    public class AppUrlConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values[parameterName] != null)
            {
                var url = values[parameterName].ToString();
                return url.Contains("/_framework/");
            }
            return false;
        }
    }
}
