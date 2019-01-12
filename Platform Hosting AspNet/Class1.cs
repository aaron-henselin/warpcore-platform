using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using WarpCore.Platform.Kernel;
using HttpContext = System.Web.HttpContext;

namespace Platform_Hosting_AspNet
{
    public class AspNetHttpRequest : IHttpRequest
    {
        public AspNetHttpRequest()
        {
           
        }

        public NameValueCollection QueryString => HttpContext.Current.Request.QueryString;

        public IDictionary<string, object> RouteTokens =>
            HttpContext.Current.Request.RequestContext.RouteData.DataTokens.ToDictionary(x => x.Key,x => x.Value);
    }

    public class AspNetWebServer : IWebServer
    {
        public string MapPath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath);
        }
    }

    public class AspNetItems : IHttpItems
    {
        public T Get<T>(string key)
        {
            return (T)HttpContext.Current.Items[key];
        }

        public void Set<T>(string key, T value)
        {
            HttpContext.Current.Items[key] = value;
        }
    }
}
