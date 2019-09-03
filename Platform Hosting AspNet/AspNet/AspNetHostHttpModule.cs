using System;
using System.Web;
using System.Web.Caching;
using Platform_Hosting_AspNet.AspNet;
using Platform_WebPipeline;
using WarpCore.Platform.Kernel;

namespace Platform_Hosting_AspNet
{
    public class AspNetHttpRuntimeCache : ICache
    {
        public void Add<T>(string cacheKey, T incomingCache, DateTime addMinutes) where T : class
        {
            HttpRuntime.Cache.Add(cacheKey, incomingCache, null, addMinutes,
                System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        public object Get<T>(string cachekey) where T:class
        {
            var cachedObject = HttpRuntime.Cache.Get(cachekey);
            return (T) cachedObject;
        }
    }
    public sealed class AspNetHostHttpModule : IHttpModule
    {
        WebPipeline _webPipeline = new WebPipeline();

        void IHttpModule.Init(HttpApplication application)
        {
            application.BeginRequest += delegate
            {
                var result = _webPipeline.DetermineWebPipelineActionForUrl(WebDependencies.Request);

                if (result is BootPage bootPage)
                {
                    HttpContext.Current.Response.StatusCode = 503;
                    HttpContext.Current.Response.Write(bootPage.LoadingHtml);
                    return;
                }

                if (result is RenderPage)
                    return;

                if (result is RewriteUrl rewriteResult)
                {
                    HttpContext.Current.RewritePath(rewriteResult.TransferUrl, true);
                    return;
                }

                if (result is Redirect redirectResult)
                {
                    HttpContext.Current.Response.Redirect(redirectResult.RedirectUrl);
                    return;
                }

                if (result is UnhandledRequest)
                    return;

                throw new NotImplementedException(nameof(AspNetHostHttpModule) + " cannot handler processor result of type " + result.GetType());

            };

            application.PostMapRequestHandler += (sender, args) =>
            {
                if (WebPipeline.CurrentRequest?.CmsPage != null)
                    HttpContext.Current.Handler = new ContentPageHandler();

            };

        }
        void IHttpModule.Dispose()
        {
        }
    }
}