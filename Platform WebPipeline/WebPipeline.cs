using WarpCore.Platform.Kernel;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace Platform_WebPipeline
{


    public class WebPipeline
    {
        public WebPipelineAction DetermineWebPipelineActionForUrl(IHttpRequest request)
        {
            if (!WebBootstrapper.IsBooted)
            {
                WebBootstrapper.EnsureSiteBootHasBeenStarted();
                return new RewriteUrl("/App_Data/Booting.aspx");
            }

            WebPipelineAction webPipelineAction = null;

            var preRequestProcessor = new WarpCoreBlazorActionBuilder(request);
            webPipelineAction = preRequestProcessor.TryProcessAsBlazorRequest();

            var hasResult = webPipelineAction != null;
            if (!hasResult)
            {
                var builder = new CmsPageRequestContextBuilder();
                var pageRequest = builder.Build(request);
                WebDependencies.RouteData.DataTokens.Add(CmsRouteDataTokens.RouteDataToken, pageRequest);

                if (pageRequest.Route != null)
                {
                    var handler = new WarpCorePageRequestActionBuilder(request);
                    webPipelineAction = handler.ProcessRequest(pageRequest);
                }
                else
                {
                    return new UnhandledRequest();
                }
            }

            return webPipelineAction;
        }

        public struct CmsRouteDataTokens
        {
            public const string RouteDataToken = "WC_RTCONTEXT";
        }

        public static CmsPageRequestContext CurrentRequest =>
            (CmsPageRequestContext)WebDependencies.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];

    }



}