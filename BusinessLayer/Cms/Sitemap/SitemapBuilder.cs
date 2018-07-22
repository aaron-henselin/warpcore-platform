using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{

    public interface ISitemapNode
    {
        Uri VirtualPath { get;  }
        List<SitemapNode> ChildNodes { get; set; }
    }

    public class Sitemap: ISitemapNode
    {
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
        public CmsPage HomePage { get; set; }
        public Uri VirtualPath => new Uri("/",UriKind.Relative);

        public ISitemapNode GetSitemapNode(CmsPage cmsPage)
        {
            return GetSitemapNode(cmsPage.ContentId.Value);
        }

        public ISitemapNode GetSitemapNode(Guid pageId)
        {
            return GetSitemapNode(pageId, this);
        }

        private static ISitemapNode GetSitemapNode(Guid pageId, ISitemapNode root)
        {
            foreach (var childNode in root.ChildNodes)
            {
                if (childNode.Page.ContentId == pageId)
                    return childNode;

                var found = GetSitemapNode(pageId, childNode);
                if (found != null)
                    return found;
            }

            return null;
        }
    }

    public class SitemapNode: ISitemapNode
    {
        public CmsPage Page { get; set; }

        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
        public Uri VirtualPath { get; set; }
    }

    public static class SitemapBuilder
    {
        public static Sitemap BuildSitemap(Site site, ContentEnvironment environment)
        {
            var siteStructure = SiteStructureMapBuilder.BuildStructureMap(site);

            var pageRepostiory = new PageRepository();
            var allPages = pageRepostiory.FindContentVersions(null, environment).Result.ToDictionary(x => x.ContentId);

            var sitemap = new Sitemap();
            if (site.HomepageId != null && allPages.ContainsKey(site.HomepageId))
                sitemap.HomePage = allPages[site.HomepageId];

            AttachChildNodes(sitemap,siteStructure,string.Empty,allPages);
            return sitemap;
        }

        private static void AttachChildNodes(ISitemapNode sitemapNode, ISiteStructureNode node, string path, Dictionary<Guid?, CmsPage> allPages)
        {

            foreach (var childStructureNode in node.ChildNodes)
            {
                var childSitemapNode = new SitemapNode();
               
                if (!allPages.ContainsKey(childStructureNode.PageId))
                    continue;
                
                childSitemapNode.Page = allPages[childStructureNode.PageId];
                childSitemapNode.VirtualPath = new Uri(path + "/" +childSitemapNode.Page.Slug,UriKind.Relative);
                sitemapNode.ChildNodes.Add(childSitemapNode);

                AttachChildNodes(childSitemapNode, childStructureNode, childSitemapNode.VirtualPath.ToString(), allPages);
            }
        }
    }





}