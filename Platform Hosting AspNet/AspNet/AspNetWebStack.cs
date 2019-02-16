using WarpCore.Web;

namespace Platform_Hosting_AspNet.AspNet
{
    

    public sealed class AspNetWebStack : WebStackConfiguration
    {
        public AspNetWebStack()
        {
            this.HttpRequest<AspNetHttpRequest>();
            this.RouteData<AspNetPerRequestRouteData>();
            this.PerRequestItems<AspNetItems>();
            this.WebServer<AspNetWebServer>();
        }

        public override void RegisterHttpModules()
        {
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(AspNetHostHttpModule));
        }
    }
}