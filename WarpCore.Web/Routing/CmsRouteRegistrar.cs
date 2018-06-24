using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.Routing;

namespace WarpCore.Cms
{
    public struct CmsRouteDataTokens
    {
        public const string RouteDataToken = "WC_ROUTE";
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
                    requestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.RouteDataToken,route);
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
            public bool IsReusable => true;

            public WarpCorePageHttpHandler()
            {
                _pageRepository = new PageRepository();
            }

            private void ProcessRequestForPage(HttpContext context, CmsPage cmsPage)
            {

                if (PageType.ContentPage == cmsPage.PageType)
                {
                    ProcessRequestForContentPage(context, cmsPage);
                    return;
                }

                if (PageType.GroupingPage == cmsPage.PageType)
                {
                    ProcessRequestForGroupingPage(context, cmsPage);
                    return;
                }

                if (PageType.RedirectPage == cmsPage.PageType)
                {
                    ProcessRequestForRedirectPage(context, cmsPage);
                    return;
                }
            }

            private void ProcessRequestForRedirectPage(HttpContext context, CmsPage cmsPage)
            {
                if (cmsPage.RedirectPageId != null)
                {
                    var firstPage = _pageRepository.Query().Single(x => x.Id == cmsPage.RedirectPageId.Value);
                    ProcessRequestForPage(context, firstPage);
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
                var firstPage = _pageRepository.Query().Where(x => x.ParentPageId == cmsPage.Id).OrderBy(x => x.SitemapPosition)
                    .SingleOrDefault();
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
                var page = _pageRepository.Query().Single(x => x.Id == siteRoute.PageId.Value);

                ProcessRequestForPage(context,page);
                
            }
        }
    }
}
