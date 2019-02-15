using System;
using System.Web;
using Platform_Hosting_AspNet.AspNet;
using Platform_WebPipeline;
using WarpCore.Platform.Kernel;

namespace Platform_Hosting_AspNet
{
    public sealed class AspNetHostHttpModule : IHttpModule
    {
        WebPipeline _webPipeline = new WebPipeline();

        void IHttpModule.Init(HttpApplication application)
        {
            application.BeginRequest += delegate
            {
                var result = _webPipeline.DetermineWebPipelineActionForUrl(WebDependencies.Request);

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