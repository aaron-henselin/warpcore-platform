using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.Routing;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public struct CmsRouteDataTokens
    {
        public const string RouteDataToken = "WC_ROUTE";
        public const string ContentEnvironmentToken = "WC_CONTENTENV";
    }

    public class CmsRouteRegistrar
    {
        public static void RegisterDynamicRoutes()
        {
            RouteTable.Routes.RouteExistingFiles = true;
            RouteTable.Routes.Add("DynamicRoute",new Route("{*url}",new WarpCorePageRouteHandler()));
        }


        private class WarpCorePageRouteHandler : IRouteHandler
        {

            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                var requestUri = requestContext.HttpContext.Request.Url;

                var success = CmsRouteTable.Current.TryGetRoute(requestUri, out var route);
                if (success)
                {
                    requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.RouteDataToken, route);

                    if ("1" == requestContext.HttpContext.Request["wc_preview"])
                        requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.ContentEnvironmentToken, ContentEnvironment.Draft);
                    else
                        requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.ContentEnvironmentToken, ContentEnvironment.Live);
                }
                else
                {
                    throw new HttpException(404,"Page cannot be found.");
                }

                return new WarpCorePageHttpHandler();
            }
        }

        private class WarpCorePageHttpHandler : IHttpHandler
        {
            private PageRepository _pageRepository;
            private SiteRepository _siteRepo;
            public bool IsReusable => true;

            public WarpCorePageHttpHandler()
            {
                _pageRepository = new PageRepository();
                _siteRepo = new SiteRepository();
            }

            private void ProcessRequestForPage(HttpContext context, CmsPage cmsPage)
            {

                if (PageType.ContentPage == cmsPage.PageType)
                {
                    ProcessRequestForContentPage(context, cmsPage);
                    return;
                }

                
                throw new ArgumentException();
            }

            private void ProcessRequestForRedirectPage(HttpContext context, CmsPage cmsPage)
            {
                if (cmsPage.RedirectPageId != null)
                {
                    context.Response.Redirect(cmsPage.RedirectExternalUrl);

                    SiteRoute route;
                    CmsRouteTable.Current.TryGetRoute(cmsPage.RedirectPageId.Value, out route);

                    //todo now: preemptively check ssl.
                    context.Response.Redirect("http://"+ route.Authority + "/" + route.VirtualPath);

                    return;
                }

                if (!string.IsNullOrWhiteSpace(cmsPage.RedirectExternalUrl))
                {
                    context.Response.Redirect(cmsPage.RedirectExternalUrl);
                    return;
                }

                throw new HttpException(404, String.Empty);
            }

            private void ProcessRequestForGroupingPage(HttpContext context, CmsPage cmsPage)
            {
               //todo: redirect manager?

                //var site = _siteRepo.GetById(cmsPage.SiteId);
                //var live = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live);

                //structure.
                //var firstPage = _pageRepository.Query().Where(x => x.ParentPageId == cmsPage.Id).OrderBy(x => x.SitemapPosition)
                //    .SingleOrDefault();
                CmsPage firstPage=null;
                if (firstPage == null)
                    throw new HttpException(404, String.Empty);

                ProcessRequestForPage(context, firstPage);
            }

            private static void ProcessRequestForContentPage(HttpContext context, CmsPage cmsPage)
            {
                var route = cmsPage.AlternateRoutes.First(x => x.Priority == (int) RoutePriority.Primary);

                if (route.VirtualPath != context.Request.Url.AbsolutePath)
                {
                    context.Response.Redirect(route.VirtualPath);
                    return;
                }
                else
                {
                    string transferUrl;

                    if (string.IsNullOrWhiteSpace(cmsPage.PhysicalFile))
                        transferUrl = cmsPage.PhysicalFile;
                    else
                        transferUrl = "DynamicPage.aspx";

                    context.Server.Transfer(transferUrl, true);
                    return;
                }
            }

            public void ProcessRequest(HttpContext context)
            {
               
                var siteRoute = (SiteRoute)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
                var contentEnvironment = (ContentEnvironment)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.ContentEnvironmentToken];

                if (siteRoute.InternalRedirectPageId != null)
                {
                    SiteRoute transferRoute;
                    CmsRouteTable.Current.TryGetRoute(siteRoute.InternalRedirectPageId.Value, out transferRoute);

                    //todo:ssl.
                    var authority = transferRoute.Authority ?? context.Request.Url.Authority;
                    context.Response.Redirect("http://"+authority+"/"+transferRoute.VirtualPath);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(siteRoute.RedirectExternalUrl))
                {
                    context.Response.Redirect(siteRoute.RedirectExternalUrl);
                    return;
                }

                var page = _pageRepository.FindContentVersions(siteRoute.PageId.Value,contentEnvironment).Result.Single();
                ProcessRequestForPage(context,page);
                
            }
        }
    }
}
