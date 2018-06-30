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
    public class GroupingPageRoute : SiteRoute
    {
        public Guid? InternalRedirectPageId { get; set; }
    }

    public class RedirectPageRoute : SiteRoute
    {
        public string RedirectExternalUrl { get; set; }
        public Guid? InternalRedirectPageId { get; set; }
    }

    public class ContentPageRoute : SiteRoute
    {
        public string ContentTypeCode { get; set; }

        public bool RequireSsl { get; set; }
    }

    public abstract class SiteRoute
    {
        public string Authority { get; set; }
        public Uri VirtualPath { get; set; }
        public int Priority { get; set; }
        public Guid SiteId { get; set; }
        public Guid? PageId { get; set; }
       
    }

    //public class RouteBuilderContext
    //{
    //    public Site CurrentSite { get; set; }
    //    public ISiteStructureNode StructureNode { get; set; }
    //    public CmsPage CurrentPage { get; set; }
    //    public string ContentRouteTemplate { get; set; }
    //    public string ContentTypeCode { get; set; }
    //    public SiteRoute AssociatedPageRoute { get; set; }
    //}

    public class RouteBuilder
    {
        

        private static IEnumerable<SiteRoute> DiscoverContentRoutes(CmsPage page, IEnumerable<ContentPageRoute> pageRoutes)
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
                        var contentRoute = new ContentPageRoute
                        {
                            Authority = pageRoute.Authority,
                            ContentTypeCode = contentRouteAttribute.ContentTypeCode,
                            PageId = pageRoute.PageId,
                            Priority = pageRoute.Priority,
                            SiteId = pageRoute.SiteId,
                            VirtualPath = new Uri(pageRoute.VirtualPath + "/"+contentRouteAttribute.RouteTemplate),
                            RequireSsl = pageRoute.RequireSsl
                        };
                        contentRoutes.Add(contentRoute);
                    }

                }

               
            }
            return contentRoutes;
        }

        private static IEnumerable<SiteRoute> DiscoverPageRoutesRecursive(SitemapNode node,Site site)
        {
            if (node.Page.PageType == null)
                throw new Exception("Undefined page type.");

            var pageRoutes = new List<SiteRoute>();

            SiteRoute primaryRoute = null;

            if (PageType.RedirectPage == node.Page.PageType)
                primaryRoute = new RedirectPageRoute
                {
                    RedirectExternalUrl = node.Page.RedirectExternalUrl,
                    InternalRedirectPageId = node.Page.RedirectPageId
                };

            if (PageType.GroupingPage == node.Page.PageType)
            {
                var first = node.ChildNodes.FirstOrDefault();
                primaryRoute = new GroupingPageRoute
                {
                    InternalRedirectPageId = first?.Page?.ContentId
                };
            }

            if (PageType.ContentPage == node.Page.PageType)
            {
                primaryRoute = new ContentPageRoute
                {
                    RequireSsl = node.Page.RequireSsl
                };

                //foreach (var route in node.Page.AlternateRoutes)
                //{
                //    var alternatePageRoute = new ContentPageRoute
                //    {
                //        Authority = site.UriAuthority,
                //        Priority = route.Priority,
                //        SiteId = site.ContentId.Value,
                //        PageId = node.Page.ContentId.Value,
                //        VirtualPath = MakeRelativeUri(site, route.VirtualPath),
                //        RequireSsl = node.Page.RequireSsl
                //    };
                //    pageRoutes.Add(alternatePageRoute);
                //}

            }

            if (primaryRoute == null)
                throw new Exception("Unsupported page type: "+node.Page.PageType);

            primaryRoute.Authority = site.UriAuthority;
            primaryRoute.Priority = (int) RoutePriority.Primary;
            primaryRoute.SiteId = site.ContentId.Value;
            primaryRoute.PageId = node.Page.ContentId.Value;
            primaryRoute.VirtualPath = MakeRelativeUri(site, node.VirtualPath);
            
            pageRoutes.Add(primaryRoute);

            var localRoutes = new List<SiteRoute>();
            if (PageType.ContentPage == node.Page.PageType)
            {
                var contentRoutes = DiscoverContentRoutes(node.Page, pageRoutes.Cast<ContentPageRoute>());
                localRoutes.AddRange(contentRoutes);
            }
            localRoutes.AddRange(pageRoutes);

            var allChildRoutes = new List<SiteRoute>();
            foreach (var child in node.ChildNodes)
            {
                var childRoutes = DiscoverPageRoutesRecursive(child, site).ToList();               
                allChildRoutes.AddRange(childRoutes);
            }

            var allRoutes = new List<SiteRoute>();
            allRoutes.AddRange(localRoutes);
            allRoutes.AddRange(allChildRoutes);

            return allRoutes;
        }

        public static IEnumerable<SiteRoute> DiscoverRoutesForSite(Site site)
        {
            var allRoutes = new List<SiteRoute>();


            var pageRepository = new PageRepository();
            var historicalPageLocations = pageRepository.GetHistoricalPageLocations(site);
foreach (var location in historicalPageLocations)
                    allRoutes.Add(new RedirectPageRoute
                    {
                        InternalRedirectPageId = location.PageId,
                        Authority = site.UriAuthority,
                        Priority = location.Priority,
                        SiteId = location.SiteId,
                        VirtualPath = new Uri(location.VirtualPath,UriKind.Relative)
                    });

            var associatedSitemap = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live);



            if (associatedSitemap.HomePage != null)
            {
                var homePageRoute = new ContentPageRoute
                {
                    Authority = site.UriAuthority,
                    Priority = 0,
                    SiteId = site.ContentId.Value,
                    PageId = site.HomepageId,
                    VirtualPath = MakeRelativeUri(site, string.Empty),
                    RequireSsl = associatedSitemap.HomePage.RequireSsl
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

        private static Uri MakeRelativeUri(Site site, string path, string contentRoute = null)
        {
            var rawUri = "/" + site.RoutePrefix + "/" + path+"/"+contentRoute;
            var nonEmptyParts = rawUri.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
            var cleanedUri = string.Join("/", nonEmptyParts);
            return new Uri(cleanedUri, UriKind.Relative);
        }

    }
}