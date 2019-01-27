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

    }
}
