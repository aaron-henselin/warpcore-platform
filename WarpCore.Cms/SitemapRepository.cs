using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
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
            return GetSitemapNode(cmsPage, this);
        }

        private static ISitemapNode GetSitemapNode(CmsPage cmsPage, ISitemapNode root)
        {
            foreach (var childNode in root.ChildNodes)
            {
                if (childNode.Page.ContentId == cmsPage.ContentId)
                    return childNode;

                var found = GetSitemapNode(cmsPage, childNode);
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
        public static Sitemap BuildSitemap(Site site, ContentEnvironment? environment)
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


    public interface ISiteStructureNode
    {
        Guid NodeId { get;  }
        IReadOnlyCollection<CmsPageLocationNode> ChildNodes { get; set; }
    }

    public class SiteStructure: ISiteStructureNode
    {
        public Guid NodeId => Guid.Empty; 
        public IReadOnlyCollection<CmsPageLocationNode> ChildNodes { get; set; } = new List<CmsPageLocationNode>();




    }

    [Unversioned]
    [Table("cms_site_structure")]
    public class CmsPageLocationNode : UnversionedContentEntity, ISiteStructureNode
    {
        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public Guid PageId { get; set; }

        [Column]
        public Guid ParentNodeId { get; set; }

        [Column]
        public Guid? BeforeNodeId { get; set; }

        public Guid NodeId { get => this.ContentId.Value; }

        [JsonIgnore]
        public IReadOnlyCollection<CmsPageLocationNode> ChildNodes { get; set; } = new List<CmsPageLocationNode>();
    }



}