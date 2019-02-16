using Platform_WebPipeline.Requests;
using WarpCore.Platform.Kernel;
using WarpCore.Web;

namespace Platform_WebPipeline
{
    public static class PageDesignerUriComponents
    {
        public const string PageId = "wc-pg";
        public const string SiteId = "wc-st";
        public const string ContentEnvironment = "wc-ce";
        public const string ViewMode = "wc-viewmode";
        public const string ContentVersion = "wc-cv";
    }

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
                var builder = new CmsPageRequestBuilder();
                var pageRequest = builder.Build(request);
                WebDependencies.PerRequestRouteData.DataTokens.Add(CmsRouteDataTokens.RouteDataToken, pageRequest);

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

        public static CmsPageRequest CurrentRequest =>
            (CmsPageRequest)WebDependencies.PerRequestRouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];

    }



}