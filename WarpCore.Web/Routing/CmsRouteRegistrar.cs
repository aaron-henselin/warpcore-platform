using System;
using System.Web;
using System.Web.Routing;

namespace WarpCore.Cms
{




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
