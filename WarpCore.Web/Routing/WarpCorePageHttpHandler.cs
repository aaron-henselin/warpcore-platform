using System;
using System.Linq;
using System.Web;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public class WarpCorePageHttpHandler : IHttpHandler
    {
        private PageRepository _pageRepository;
        private SiteRepository _siteRepo;
        public bool IsReusable => true;

        public WarpCorePageHttpHandler()
        {
            _pageRepository = new PageRepository();
            _siteRepo = new SiteRepository();
        }

        //private void ProcessContentPageRequest(HttpContext context, CmsPage cmsPage)
        //{

        //    if (PageType.ContentPage == cmsPage.PageType)
        //    {
        //        ProcessRequestForContentPage(context, cmsPage);
        //        return;
        //    }


        //    throw new ArgumentException();
        //}

        //private void ProcessRequestForRedirectPage(HttpContext context, CmsPage cmsPage)
        //{
        //    if (cmsPage.RedirectPageId != null)
        //    {
        //        context.Response.Redirect(cmsPage.RedirectExternalUrl);

        //        SiteRoute route;
        //        CmsRouteTable.Current.TryGetRoute(cmsPage.RedirectPageId.Value, out route);

        //        var redirectUrl = CreateUrl(route,context);
        //        context.Response.Redirect(redirectUrl);

        //        return;
        //    }

        //    if (!string.IsNullOrWhiteSpace(cmsPage.RedirectExternalUrl))
        //    {
        //        context.Response.Redirect(cmsPage.RedirectExternalUrl);
        //        return;
        //    }

        //    throw new HttpException(404, String.Empty);
        //}

        //private void ProcessRequestForGroupingPage(HttpContext context, CmsPage cmsPage)
        //{
        //    //todo: redirect manager?

        //    //var site = _siteRepo.GetById(cmsPage.SiteId);
        //    //var live = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live);

        //    //structure.
        //    //var firstPage = _pageRepository.Query().Where(x => x.ParentPageId == cmsPage.Id).OrderBy(x => x.SitemapPosition)
        //    //    .SingleOrDefault();
        //    CmsPage firstPage = null;
        //    if (firstPage == null)
        //        throw new HttpException(404, String.Empty);

        //    ProcessContentPageRequest(context, firstPage);
        //}

        private static void ProcessRequestForContentPage(HttpContext context, CmsPage cmsPage)
        {
            //var route = cmsPage.AlternateRoutes.First(x => x.Priority == (int)RoutePriority.Primary);

            //if (route.VirtualPath != context.Request.Url.AbsolutePath)
            //{
            //    context.Response.Redirect(route.VirtualPath);
            //    return;
            //}
            //else
            //{
                string transferUrl;

                if (string.IsNullOrWhiteSpace(cmsPage.PhysicalFile))
                    transferUrl = cmsPage.PhysicalFile;
                else
                    transferUrl = "DynamicPage.aspx";

                context.Server.Transfer(transferUrl, true);
                return;
            //}
        }

        private string CreateUrl(SiteRoute transferRoute, HttpContext httpContext)
        {
            bool ssl = httpContext.Request.Url.Scheme == "https";
            if (transferRoute is ContentPageRoute route)
            {
                ssl = route.RequireSsl;
            }

            var protocol = "http://";
            if (ssl)
                protocol = "https://";

            var authority = transferRoute.Authority;
            if (string.IsNullOrWhiteSpace(authority))
                authority = httpContext.Request.Url.Authority;

            return protocol + authority + "/" + transferRoute.VirtualPath;
        }

        public void ProcessRequest(HttpContext context)
        {
            var siteRoute = (SiteRoute)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
            var contentEnvironment = (ContentEnvironment)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.ContentEnvironmentToken];

 
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

            var page = _pageRepository.FindContentVersions(By.ContentId(siteRoute.PageId.Value), contentEnvironment).Result.Single();
            ProcessRequestForContentPage(context, page);

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