using System;
using System.Linq;
using System.Web;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace WarpCore.Cms
{
    public  class WarpCoreRequestProcessor
    {
        private static void ProcessRequestForContentPage(HttpContext context, CmsPage cmsPage)
        {
            string transferUrl;

            if (!string.IsNullOrWhiteSpace(cmsPage.PhysicalFile))
                transferUrl = cmsPage.PhysicalFile;
            else
                transferUrl = "/App_Data/DynamicPage.aspx";

            context.RewritePath(transferUrl,true);
        }

        private string CreateUrl(SiteRoute transferRoute, HttpContext httpContext)
        {
            var uriBuilderContext = httpContext.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            return uriBuilder.CreateUriForRoute(transferRoute, UriSettings.Default).ToString();
   
        }

        public void ProcessRequest(HttpContext context, CmsPageRequestContext pageRequestContext)
        {
            //var siteRoute = (SiteRoute)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
            //var contentEnvironment = (ContentEnvironment)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.ContentEnvironmentToken];

            var siteRoute = pageRequestContext.Route;

            if (siteRoute is RedirectPageRoute)
            {
                var redirectRoute = (RedirectPageRoute)siteRoute;
                ProcessRedirectPageRequest(context, redirectRoute);
                return;
            }


            if (siteRoute is GroupingPageRoute)
            {
                var redirectRoute = (GroupingPageRoute)siteRoute;
                ProcessGroupingPageRequest(context, redirectRoute);
                return;
            }

          
            ProcessRequestForContentPage(context, pageRequestContext.CmsPage);

        }

        private void ProcessGroupingPageRequest(HttpContext context, GroupingPageRoute redirectRoute)
        {
            if (redirectRoute.InternalRedirectPageId == null)
                throw new HttpException(404, "No page id.");

            SiteRoute redirectToRoute;
            CmsRoutes.Current.TryResolveRoute(redirectRoute.InternalRedirectPageId.Value, out redirectToRoute);

            var redirectUrl = CreateUrl(redirectToRoute, context);
            context.Response.Redirect(redirectUrl);
        }

        private void ProcessRedirectPageRequest(HttpContext context, RedirectPageRoute redirectRoute)
        {
            if (!string.IsNullOrWhiteSpace(redirectRoute.RedirectExternalUrl))
            {
                context.Response.Redirect(redirectRoute.RedirectExternalUrl);
                return;
            }

            if (redirectRoute.InternalRedirectPageId == null)
                throw new HttpException(404,"No page id.");

            SiteRoute redirectToRoute;
            CmsRoutes.Current.TryResolveRoute(redirectRoute.InternalRedirectPageId.Value, out redirectToRoute);

            var redirectUrl = CreateUrl(redirectToRoute, context);
            context.Response.Redirect(redirectUrl);
            return;
        }
    }
}