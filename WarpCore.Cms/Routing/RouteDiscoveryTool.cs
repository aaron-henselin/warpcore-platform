using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarpCore.Cms.Content;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;

namespace WarpCore.Cms
{
    public class SiteRoute
    {
        public string Authority { get; set; }
        public Uri VirtualPath { get; set; }
        public int Priority { get; set; }
        public Guid SiteId { get; set; }
        //public Guid? ContentId { get; set; }
        public Guid? PageId { get; set; }

        public string ContentTypeCode { get; set; }
    }

    public class RouteDiscoveryContext
    {
        public Site Site { get; set; }
        public CmsPage CmsPage { get; set; }
        public string ContentRouteTemplate { get; set; }
        public string ContentTypeCode { get; set; }
        public SiteRoute AssociatedPageRoute { get; set; }
    }

    public static class RouteDiscoveryUtility
    {
        //private static IEnumerable<SiteRoute> DiscoverRoutesForPageContent(RouteDiscoveryContext context)
        //{

        //    List<SiteRoute> pageContentRoutes = new List<SiteRoute>();
        //    var contentTypeCode = context.ContentTypeCode;
        //    var dynamicContentManager = new CmsContentManager();
        //    var allContent = dynamicContentManager.GetAll(contentTypeCode);

        //    //todo: better route templates;
        //    var routeTemplate = context.ContentRouteTemplate;
        //    if (string.IsNullOrWhiteSpace(routeTemplate))
        //        routeTemplate = "{Title}";

        //    foreach (var item in allContent)
        //    {
        //        var contentAddendum =
        //            routeTemplate.Replace("{Title}", SlugGenerator.Generate(item.Name));

        //        var pageContentRoute = new SiteRoute
        //        {
        //            Authority = context.Site.UriAuthority,
        //            Priority = (int)RoutePriority.Primary,
        //            SiteId = context.Site.Id,
        //            ContentId = item.Id,
        //            PageId = context.CmsPage.Id,
        //            VirtualPath = MakeAbsoluteUri(context.Site, context.AssociatedPageRoute.VirtualPath.ToString(), contentAddendum)
        //        };

        //        pageContentRoutes.Add(pageContentRoute);
        //    }

        //    return pageContentRoutes;
        //}

        private static IEnumerable<SiteRoute> DiscoverRoutesForPage(RouteDiscoveryContext context)
        {
            var siteRoutes = new List<SiteRoute>();
            foreach (var route in context.CmsPage.Routes)
            {
                var pageRoute = new SiteRoute
                {
                    Authority = context.Site.UriAuthority,
                    Priority = route.Priority,
                    SiteId = context.Site.Id,
                    ContentTypeCode = null,
                    PageId = context.CmsPage.Id,
                    VirtualPath = MakeAbsoluteUri(context.Site, route.Slug)
                };

                siteRoutes.Add(pageRoute);

                //////////////////////////

                foreach (var content in context.CmsPage.PageContent)
                {
                    var toolboxManager = new ToolboxManager();
                    var toolboxItem = toolboxManager.GetToolboxItemByCode(content.WidgetTypeCode);
                    var toolboxItemType = Type.GetType(toolboxItem.FullyQualifiedTypeName);
                    var contentRouteBuilders = toolboxItemType.GetCustomAttributes(typeof(ContentRouteAttribute)).Cast<ContentRouteAttribute>().ToList();


                    foreach (var contentRouteBuilder in contentRouteBuilders)
                    {
                        var contentRoute = new SiteRoute
                        {
                            Authority = pageRoute.Authority,
                            ContentTypeCode = contentRouteBuilder.ContentTypeCode,
                            PageId = pageRoute.PageId,
                            Priority = pageRoute.Priority,
                            SiteId = pageRoute.SiteId,
                            VirtualPath = MakeAbsoluteUri(context.Site, pageRoute.VirtualPath.ToString(),"{Title}") 
                        };
                        siteRoutes.Add(contentRoute);

                        //var discoveryContext =
                        //new RouteDiscoveryContext
                        //{
                        //    CmsPage = context.CmsPage,
                        //    Site = context.Site,
                        //    AssociatedPageRoute = pageRoute,
                        //    ContentRouteTemplate = contentRoute.RouteTemplate,
                        //    ContentTypeCode = contentRoute.ContentTypeCode,
                        //};

                        //var pageContentRoutes = DiscoverRoutesForPageContent(discoveryContext);
                        //siteRoutes.AddRange(pageContentRoutes);

                    }

                }
            }

            return siteRoutes;
        }

        private static IEnumerable<SiteRoute> DiscoverRoutesForSite(RouteDiscoveryContext context)
        {
            var site = context.Site;
            List<SiteRoute> siteRoutes = new List<SiteRoute>();

            var pageRepo = new PageRepository();
            var foundPages = pageRepo.Query(site);
            
            var sitePages = foundPages;

            var homePageRoute = new SiteRoute
            {
                Authority = site.UriAuthority,
                Priority = 0,
                SiteId = site.Id,
                PageId = site.HomepagePageId,
                VirtualPath = MakeAbsoluteUri(site, string.Empty)
            };

            siteRoutes.Add(homePageRoute);

            foreach (var page in sitePages)
            {
                var pageRoutes = DiscoverRoutesForPage(new RouteDiscoveryContext { Site=site,CmsPage=page});
                siteRoutes.AddRange(pageRoutes);

            }

            return siteRoutes;
        }

        public static IEnumerable<SiteRoute> DiscoverRoutes()
        {
            List<SiteRoute> allRoutes = new List<SiteRoute>();

            var sites = new SiteRepository()
                .GetAllSites();

            foreach (var site in sites)
            {
                var siteRoutes = DiscoverRoutesForSite(new RouteDiscoveryContext {Site=site});
                allRoutes.AddRange(siteRoutes);
            }

            return allRoutes;
        }
        
        private static Uri MakeAbsoluteUri(Site site, string path, string contentRoute = null)
        {
            var rawUri = "/" + site.RoutePrefix + "/" + path+"/"+contentRoute;
            var nonEmptyParts = rawUri.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
            var cleanedUri = string.Join("/", nonEmptyParts);
            return new Uri(cleanedUri, UriKind.Relative);
        }

    }
}