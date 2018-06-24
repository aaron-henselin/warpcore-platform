using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarpCore.Cms.Content;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;

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

    public class RouteBuilderContext
    {
        public Site CurrentSite { get; set; }
        public ISiteStructureNode StructureNode { get; set; }
        public CmsPage CurrentPage { get; set; }
        public string ContentRouteTemplate { get; set; }
        public string ContentTypeCode { get; set; }
        public SiteRoute AssociatedPageRoute { get; set; }
    }

    public class RouteBuilder
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

        private static IEnumerable<SiteRoute> DiscoverContentRoutes(CmsPage page, IEnumerable<SiteRoute> pageRoutes)
        {
            var contentRoutes = new List<SiteRoute>();

            foreach (var content in page.PageContent)
            {
                var toolboxManager = new ToolboxManager();
                var toolboxItem = toolboxManager.GetToolboxItemByCode(content.WidgetTypeCode);
                var toolboxItemType = Type.GetType(toolboxItem.FullyQualifiedTypeName);
                var contentRouteAttributes = toolboxItemType.GetCustomAttributes(typeof(ContentRouteAttribute)).Cast<ContentRouteAttribute>().ToList();

                foreach (var contentRouteAttribute in contentRouteAttributes)
                {
                    var pageRoutesToCopy = pageRoutes.ToList();
                    foreach (var pageRoute in pageRoutesToCopy)
                    {
                        var contentRoute = new SiteRoute
                        {
                            Authority = pageRoute.Authority,
                            ContentTypeCode = contentRouteAttribute.ContentTypeCode,
                            PageId = pageRoute.PageId,
                            Priority = pageRoute.Priority,
                            SiteId = pageRoute.SiteId,
                            VirtualPath = new Uri(pageRoute.VirtualPath + "/"+contentRouteAttribute.RouteTemplate)
                        };
                        contentRoutes.Add(contentRoute);
                    }

                }

               
            }
            return contentRoutes;
        }

        private static IEnumerable<SiteRoute> DiscoverPageRoutesRecursive(SitemapNode node,Site site)
        {
            var pageRoutes = new List<SiteRoute>();

            var primaryRoute = new SiteRoute
            {
                Authority = site.UriAuthority,
                Priority = (int)RoutePriority.Primary,
                SiteId = site.ContentId.Value,
                ContentTypeCode = null,
                PageId = node.Page.ContentId.Value,
                VirtualPath = MakeAbsoluteUri(site, node.VirtualPath)
            };
            pageRoutes.Add(primaryRoute);

            foreach (var route in node.Page.AlternateRoutes)
            {
                var alternatePageRoute = new SiteRoute
                {
                    Authority = site.UriAuthority,
                    Priority = route.Priority,
                    SiteId = site.ContentId.Value,
                    ContentTypeCode = null,
                    PageId = node.Page.ContentId.Value,
                    VirtualPath = MakeAbsoluteUri(site, route.VirtualPath)
                };
                pageRoutes.Add(alternatePageRoute);
            }

            var contentRoutes = DiscoverContentRoutes(node.Page, pageRoutes);

            var localRoutes = new List<SiteRoute>();
            localRoutes.AddRange(contentRoutes);
            localRoutes.AddRange(pageRoutes);

            var allChildRoutes = new List<SiteRoute>();
            foreach (var child in node.ChildNodes)
            {
                var childRoutes = DiscoverPageRoutesRecursive(child, site);
                allChildRoutes.AddRange(childRoutes);
            }

            var allRoutes = new List<SiteRoute>();
            allRoutes.AddRange(localRoutes);
            allRoutes.AddRange(allChildRoutes);

            return allRoutes;
        }

        private static IEnumerable<SiteRoute> DiscoverRoutesForSite(Site site)
        {
            var associatedSitemap = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live);


            var allRoutes = new List<SiteRoute>();

            if (associatedSitemap.HomePage != null)
            {
                var homePageRoute = new SiteRoute
                {
                    Authority = site.UriAuthority,
                    Priority = 0,
                    SiteId = site.ContentId.Value,
                    PageId = site.HomepageId,
                    VirtualPath = MakeAbsoluteUri(site, string.Empty)
                };

                var homepageContentRoutes = DiscoverContentRoutes(associatedSitemap.HomePage, new[] {homePageRoute});
                allRoutes.Add(homePageRoute);
                allRoutes.AddRange(homepageContentRoutes);
            }

            foreach (var childNode in associatedSitemap.ChildNodes)
            {
                allRoutes.AddRange(DiscoverPageRoutesRecursive(childNode,site));
            }

            return allRoutes;
        }

        //public static IEnumerable<SiteRoute> DiscoverRoutes(Site site)
        //{
        //    List<SiteRoute> allRoutes = new List<SiteRoute>();
        //    var siteRoutes = DiscoverRoutesForSite(new RouteBuilderContext {CurrentSite = site});
        //    allRoutes.AddRange(siteRoutes);
        //    return allRoutes;
        //}

        private static Uri MakeAbsoluteUri(Site site, string path, string contentRoute = null)
        {
            var rawUri = "/" + site.RoutePrefix + "/" + path+"/"+contentRoute;
            var nonEmptyParts = rawUri.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
            var cleanedUri = string.Join("/", nonEmptyParts);
            return new Uri(cleanedUri, UriKind.Relative);
        }

    }
}