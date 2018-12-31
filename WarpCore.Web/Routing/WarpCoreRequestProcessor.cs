using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Web;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets;

namespace WarpCore.Cms
{
    public class ContentPageHandler : IHttpHandler
    {
        public ContentPageHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            var rq = CmsPageRequestContext.Current;

            RenderContentPage(rq);
        }


        private static void RenderContentPage(CmsPageRequestContext rt)
        {
            var pageBuilder = new CmsPageLayoutEngine(rt);

            var page = new PageRendering();

            pageBuilder.ActivateAndPlaceLayoutContent(page);

            var pageSpecificContent = rt.CmsPage.PageContent;
            if (rt.PageRenderMode == PageRenderMode.PageDesigner)
            {
                var editing = new EditingContextManager();
                var context = editing.GetOrCreateEditingContext(rt.CmsPage);
                pageSpecificContent = context.AllContent;
            }

            var d = page.GetPartialPageRenderingByLayoutBuilderId();

            foreach (var contentItem in pageSpecificContent)
            {
                var placementLayoutBuilderId = contentItem.PlacementLayoutBuilderId ?? Guid.Empty;
                var root = d[placementLayoutBuilderId];
                pageBuilder.ActivateAndPlaceAdHocPageContent(root, contentItem);

            }

            var cre = new CompositeRenderingEngine();
            var batch = cre.Execute(page,
                rt.PageRenderMode);

            HttpContext.Current.Response.Write(batch.Html);
        }



        public bool IsReusable { get; } = false;
    }

    public  class WarpCoreRequestProcessor
    {
        private static void ProcessRequestForContentPage(HttpContext context, CmsPage cmsPage)
        {
            string transferUrl;

            if (!string.IsNullOrWhiteSpace(cmsPage.PhysicalFile))
            {
                transferUrl = cmsPage.PhysicalFile;
                context.RewritePath(transferUrl, false);
            }
            else
            {
           
                
            }
            

        }


        private string CreateUrl(SiteRoute transferRoute, HttpContext httpContext, IDictionary<string,string> parameters)
        {
            var uriBuilderContext = httpContext.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            return uriBuilder.CreateUriForRoute(transferRoute, UriSettings.Default, parameters).ToString();
   
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

            var redirectUrl = CreateUrl(redirectToRoute, context, null);
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

            var redirectUrl = CreateUrl(redirectToRoute, context,redirectRoute.InternalRedirectParameters);
            context.Response.Redirect(redirectUrl);
            return;
        }
    }
}