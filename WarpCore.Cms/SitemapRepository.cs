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

    [Table("cms_sitemap")]
    public class Sitemap : CosmosEntity, ISitemapNode
    {
        public string Name { get; set; }

        public Guid HomepageId { get; set; }

        [JsonIgnore]
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
        public Guid NodeId { get => this.ContentId.Value; }
    }


    [Table("cms_sitemap_node")]
    public class SitemapNode : CosmosEntity, ISitemapNode
    {
        [Column]
        public Guid SitemapId { get; set; }

        [Column]
        public Guid PageId { get; set; }

        [Column]
        public Guid ParentNodeId { get; set; }

        [JsonIgnore]
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();

        public Guid NodeId { get => this.ContentId.Value; }
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


        public void GetSitemap(string sitemapName)
        {
            var sitemapLookup = $"{nameof(Sitemap.Name)} eq '{sitemapName}'";
            var sitemap = _orm.FindContentVersions<Sitemap>(sitemapLookup, ContentEnvironment.Unversioned).Result.Single();

            var pageLookup = $"{nameof(SitemapNode.SitemapId)} eq '{sitemap.ContentId.Value}'";
            var allpages = _orm.FindContentVersions<SitemapNode>(pageLookup, ContentEnvironment.Unversioned).Result.ToList();

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