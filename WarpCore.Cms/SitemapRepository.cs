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
        List<SitemapNode> ChildNodes { get; set; }
    }

    public class Sitemap: ISitemapNode
    {
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }

    public class SitemapNode: ISitemapNode
    {
        public CmsPage Page { get; set; }

        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }

    public static class SitemapBuilder
    {
        public static void BuildSitemap(Site site, ContentEnvironment? environment)
        {
            var siteRepository= new SiteRepository();
            var siteStructure = siteRepository.GetSiteStructure(site);

            var pageRepostiory = new PageRepository();
            var allPages = pageRepostiory.FindContentVersions(null, environment).Result.ToDictionary(x => x.ContentId);

            var sitemap = new Sitemap();
            ConvertChildNodes(sitemap,siteStructure,allPages);
        }

        private static void ConvertChildNodes(ISitemapNode sitemapNode, ISiteStructureNode node, Dictionary<Guid?, CmsPage> allPages)
        {

            foreach (var childStructureNode in node.ChildNodes)
            {
                var childSitemapNode = new SitemapNode();
                sitemapNode.ChildNodes.Add(childSitemapNode);
                if (allPages.ContainsKey(childStructureNode.PageId))
                    childSitemapNode.Page = allPages[childStructureNode.PageId];

                ConvertChildNodes(childSitemapNode,childStructureNode,allPages);
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
        public Guid NodeId { get; set; }
        public List<SiteStructureNode> ChildNodes { get; set; } = new List<SiteStructureNode>();
    }

    [Unversioned]
    [Table("cms_site_structure")]
    public class SiteStructureNode : CosmosEntity, ISiteStructureNode
    {
        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public Guid PageId { get; set; }

        [Column]
        public Guid ParentNodeId { get; set; }


        public Guid NodeId { get => this.ContentId.Value; }
        [JsonIgnore]
        public List<SiteStructureNode> ChildNodes { get; set; } = new List<SiteStructureNode>();
    }



}