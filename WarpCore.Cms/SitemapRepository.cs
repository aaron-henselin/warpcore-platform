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
        string VirtualPath { get;  }
        List<SitemapNode> ChildNodes { get; set; }
    }

    public class Sitemap: ISitemapNode
    {
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
        public CmsPage HomePage { get; set; }
        public string VirtualPath => "/";
    }

    public class SitemapNode: ISitemapNode
    {
        public CmsPage Page { get; set; }

        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
        public string VirtualPath { get; set; }
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

            AttachChildNodes(sitemap,siteStructure,"",allPages);
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
                childSitemapNode.VirtualPath = path + "/" + childSitemapNode.Page.Slug;
                sitemapNode.ChildNodes.Add(childSitemapNode);

                AttachChildNodes(childSitemapNode, childStructureNode, childSitemapNode.VirtualPath, allPages);
            }
        }
    }


    public interface ISiteStructureNode
    {
        Guid NodeId { get;  }
        List<SiteStructureNode> ChildNodes { get; set; }
    }

    public class SiteStructure: ISiteStructureNode
    {
        public Guid NodeId => Guid.Empty; 
        public List<SiteStructureNode> ChildNodes { get; set; } = new List<SiteStructureNode>();




    }

    [Unversioned]
    [Table("cms_site_structure")]
    public class SiteStructureNode : UnversionedContentEntity, ISiteStructureNode
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
        public List<SiteStructureNode> ChildNodes { get; set; } = new List<SiteStructureNode>();
    }



}