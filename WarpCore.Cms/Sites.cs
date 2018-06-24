using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    [Unversioned]
    [Table("cms_site")]
    public class Site : CosmosEntity
    {
        public string Name { get; set; }
        public string RoutePrefix { get; set; }
        public string UriAuthority { get; set; }
        public int Priority { get; set; }

        public Guid? HomepageId { get; set; }
    }

    public class SiteRepository : CosmosRepository<Site>
    {
        public void MovePage(CmsPage page, Guid toPositionId)
        {
            var condition = $@"{nameof(SiteStructureNode.PageId)} eq '{page.ContentId}'";
            var sitemapNode = _orm.FindContentVersions<SiteStructureNode>(condition, null).Result.SingleOrDefault();
            if (sitemapNode == null)
                sitemapNode = new SiteStructureNode();

            sitemapNode.PageId = page.ContentId.Value;
            sitemapNode.SiteId = page.SiteId;
            sitemapNode.ParentNodeId = toPositionId;

            _orm.Save(sitemapNode);
        }

        public SiteStructure GetSiteStructure(Site site)
        {
            var sitemapLookup = $"{nameof(SiteStructureNode.SiteId)} eq '{site.ContentId}'";
            var allpages = _orm.FindContentVersions<SiteStructureNode>(sitemapLookup, ContentEnvironment.Unversioned).Result.ToList();

            var sitemap = new SiteStructure();
            var parentNodeLookup = allpages.ToLookup(x => x.ParentNodeId);
            PopulateChildNodes(sitemap, parentNodeLookup);

            return sitemap;
        }

        private static void PopulateChildNodes(ISiteStructureNode sitemap, ILookup<Guid, SiteStructureNode> parentNodeLookup)
        {
            sitemap.ChildNodes = parentNodeLookup[sitemap.NodeId].ToList();

            foreach (var childNode in sitemap.ChildNodes)
                PopulateChildNodes(childNode, parentNodeLookup);

        }
    }


}
