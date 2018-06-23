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
        Guid NodeId { get;  }
        List<SitemapNode> ChildNodes { get; set; }
    }

    public class Sitemap: ISitemapNode
    {
        public Guid NodeId { get; set; }
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }

    [Table("cms_site_structure_node")]
    public class SitemapNode : CosmosEntity, ISitemapNode
    {
        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public Guid PageId { get; set; }

        [Column]
        public Guid ParentNodeId { get; set; }


        public Guid NodeId { get => this.ContentId.Value; }
        [JsonIgnore]
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }


    public class SitemapRepository
    {
        private ICosmosOrm _orm;

        public SitemapRepository()
        {
            _orm = Dependency.Resolve<ICosmosOrm>();
        }

        public void MovePage(CmsPage page, Guid toPositionId)
        {
            var condition = $@"{nameof(SitemapNode.PageId)} eq '{page.ContentId}'";
            var pageToUpdate = _orm.FindContentVersions<SitemapNode>(condition,null).Result.Single();
            pageToUpdate.ParentNodeId = toPositionId;
            _orm.Save(pageToUpdate);
        }


        public void GetSitemap(Site site)
        {
            var sitemapLookup = $"{nameof(SitemapNode.SiteId)} eq '{site.Id}'";
            var allpages = _orm.FindContentVersions<SitemapNode>(sitemapLookup, ContentEnvironment.Unversioned).Result.ToList();

            var sitemap = new Sitemap();
            var parentNodeLookup = allpages.ToLookup(x => x.ParentNodeId);
            PopulateChildNodes(sitemap, parentNodeLookup);
        }

        private static void PopulateChildNodes(ISitemapNode sitemap, ILookup<Guid, SitemapNode> parentNodeLookup)
        {
            sitemap.ChildNodes = parentNodeLookup[sitemap.NodeId].ToList();

            foreach (var childNode in sitemap.ChildNodes)
                PopulateChildNodes(childNode,parentNodeLookup);
            
        }
    }
}