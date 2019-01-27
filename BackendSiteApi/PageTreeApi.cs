using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using BlazorComponents.Shared;
using Cms_PageDesigner_Context;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

[assembly:IsWarpCorePluginAssembly]
namespace BackendSiteApi
{
    public class PageTreeApiController : ApiController
    {
        [HttpPost]
        public void Publish(Guid id)
        {
            var cmsPageRepository = new CmsPageRepository();
            cmsPageRepository.Publish(By.ContentId(id));
        }

        [HttpGet]
        public IReadOnlyCollection<SitemapModel> Sites()
        {
            var sites = new SiteRepository().Find().ToList();
            return BuildPageTreeModels(sites);
        }

        private List<SitemapModel> BuildPageTreeModels(List<Site> sites)
        {
            List<SitemapModel> pageTreees = new List<SitemapModel>();
            foreach (var site in sites)
            {
                var pageTree = BuildPageTreeItem(site);
                pageTreees.Add(pageTree);
            }

            return pageTreees;
        }

        private SitemapModel BuildPageTreeItem(Site site)
        {
            var model = new SitemapModel();
            model.SiteId = site.ContentId;
            model.SiteName = site.Name;
            var draftSitemap = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Draft, SitemapBuilderFilters.All);

            foreach (var childNode in draftSitemap.ChildNodes)
                model.ChildNodes.Add(CreateSiteMapNode(childNode, 0));

            var liveSitemap = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live, SitemapBuilderFilters.All);
            var flatItems = model.GetAllDescendents<SitemapModelNode>();
            foreach (var item in flatItems)
            {
                var liveNode = liveSitemap.GetSitemapNode(item.PageId);
                item.IsPublished = liveNode != null;
                item.IsHomePage = item.PageId == site.HomepageId;
            }

            return model;
        }

        private SitemapModelNode CreateSiteMapNode(SitemapNode sitemapNode, int depth)
        {
            var node = new SitemapModelNode
            {
                Id = sitemapNode.Page.ContentId,
                Name = sitemapNode.Page.Name,
                PageId = sitemapNode.Page.ContentId,
                VirtualPath = sitemapNode.VirtualPath.ToString(),
                Depth = depth,
                ParentPath = sitemapNode?.VirtualPath.ToString(),
                HasChildItems = sitemapNode.ChildNodes.Any(),
                ChildNodes = sitemapNode.ChildNodes.Select(x => CreateSiteMapNode(x,depth+1)).ToList()
            };

            var uriBuilder = new CmsUriBuilder();

            Guid settingsPageId = Guid.Empty;
            var draftNode = sitemapNode;
            if (draftNode.Page.PageType == PageType.ContentPage)
            {
                try
                {
                    var uri = uriBuilder.CreateUri(draftNode.Page, UriSettings.Default, new Dictionary<string, string>
                    {
                        [PageDesignerUriComponents.ViewMode] = "PageDesigner",
                    });
                    node.DesignUrl = uri.ToString();
                }
                catch (Exception e)
                {
                    node.DesignUrl = $"/Admin/draft?{PageDesignerUriComponents.ViewMode}=PageDesigner&{PageDesignerUriComponents.SiteId}={draftNode.Page.SiteId}&{PageDesignerUriComponents.PageId}={node.PageId}";
                }

                settingsPageId = KnownPageIds.ContentPageSettings;
            }
            if (draftNode.Page.PageType == PageType.GroupingPage)
                settingsPageId = KnownPageIds.GroupingPageSettings;

            if (draftNode.Page.PageType == PageType.RedirectPage)
                settingsPageId = KnownPageIds.RedirectPageSettings;


            var success = CmsRoutes.Current.TryResolveRoute(settingsPageId, out var sr);
            if (success)
                node.SettingsUrl = uriBuilder.CreateUriForRoute(sr, UriSettings.Default, new Dictionary<string, string> { ["contentId"] = node.PageId.ToString() }).ToString();



            return node;
        }


        //private List<PageTreeItem> BuildPageTreeItem(Site matchedSite)
        //{

        //    var draftSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Draft, SitemapBuilderFilters.All);
        //    var liveSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Live, SitemapBuilderFilters.All);

        //    var pagesTreeItems = new List<PageTreeItem>();
        //    foreach (var childNode in draftSitemap.ChildNodes)
        //    {
        //        FlattenPageTree(childNode, pagesTreeItems, 0);
        //    }

        //    var uriBuilder = new CmsUriBuilder();


        //    foreach (var pageTreeItem in pagesTreeItems)
        //    {
        //        var liveNode = liveSitemap.GetSitemapNode(pageTreeItem.PageId);
        //        pageTreeItem.IsPublished = liveNode != null;
        //        pageTreeItem.IsHomePage = pageTreeItem.PageId == matchedSite.HomepageId;


        //        Guid settingsPageId = Guid.Empty;
        //        var draftNode = draftSitemap.GetSitemapNode(pageTreeItem.PageId);
        //        if (draftNode.Page.PageType == PageType.ContentPage)
        //        {
        //            try
        //            {
        //                var uri = uriBuilder.CreateUri(draftNode.Page, UriSettings.Default, new Dictionary<string, string>
        //                {
        //                    [PageDesignerUriComponents.ViewMode] = "PageDesigner",
        //                });
        //                pageTreeItem.DesignUrl = uri.ToString();
        //            }
        //            catch (Exception e)
        //            {
        //                pageTreeItem.DesignUrl = $"/Admin/draft?{PageDesignerUriComponents.ViewMode}=PageDesigner&{PageDesignerUriComponents.SiteId}={draftNode.Page.SiteId}&{PageDesignerUriComponents.PageId}={pageTreeItem.PageId}";
        //            }

        //            settingsPageId = KnownPageIds.ContentPageSettings;
        //        }
        //        if (draftNode.Page.PageType == PageType.GroupingPage)
        //            settingsPageId = KnownPageIds.GroupingPageSettings;

        //        if (draftNode.Page.PageType == PageType.RedirectPage)
        //            settingsPageId = KnownPageIds.RedirectPageSettings;


        //        var success = CmsRoutes.Current.TryResolveRoute(settingsPageId, out var sr);
        //        if (success)
        //            pageTreeItem.SettingsUrl = uriBuilder.CreateUriForRoute(sr, UriSettings.Default, new Dictionary<string, string> { ["contentId"] = pageTreeItem.PageId.ToString() }).ToString();

        //    }

        //    return pagesTreeItems.ToList();

        //    //_controlState.PageTreeItems = pagesTreeItems.ToList();

        //    //foreach (var item in _controlState.PageTreeItems)
        //    //{
        //    //    if (item.ParentPath == null)
        //    //    {
        //    //        item.Visible = true;
        //    //        ExpandPath(item.VirtualPath.ToString());
        //    //    }
        //    //}
        //}


        //private void FlattenPageTree(SitemapNode sitemapNode, List<PageTreeItem> nodes, int depth, SitemapNode parentNode = null)
        //{

        //    nodes.Add(new PageTreeItem
        //    {
        //        //SitemapNode = sitemapNode,
        //        Name = sitemapNode.Page.Name,
        //        PageId = sitemapNode.Page.ContentId,
        //        VirtualPath = sitemapNode.VirtualPath.ToString(),
        //        Depth = depth,
        //        ParentPath = parentNode?.VirtualPath.ToString(),
        //        HasChildItems = sitemapNode.ChildNodes.Any()
        //    });

        //    foreach (var childNode in sitemapNode.ChildNodes)
        //        FlattenPageTree(childNode, nodes, depth + 1, sitemapNode);

        //}

    }
}
