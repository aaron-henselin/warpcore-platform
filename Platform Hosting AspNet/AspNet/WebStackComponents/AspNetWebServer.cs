using Platform_WebPipeline;
using WarpCore.Platform.Kernel;
using HttpContext = System.Web.HttpContext;

namespace Platform_Hosting_AspNet
{
    public class AspNetWebServer : IWebServer
    {
        public string MapPath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath);
        }

    }
}
