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
    }

    public class SiteRepository : CosmosRepository<Site>
    {
        

        public void MovePage(CmsPage page, Guid toPositionId)
        {
            var condition = $@"{nameof(SitemapNode.PageId)} eq '{page.ContentId}'";
            var pageToUpdate = _orm.FindContentVersions<SitemapNode>(condition, null).Result.Single();
            pageToUpdate.ParentNodeId = toPositionId;
            _orm.Save(pageToUpdate);
        }

        public Sitemap GetSitemap(Site site)
        {
            var sitemapLookup = $"{nameof(SitemapNode.SiteId)} eq '{site.ContentId}'";
            var allpages = _orm.FindContentVersions<SitemapNode>(sitemapLookup, ContentEnvironment.Unversioned).Result.ToList();

            var sitemap = new Sitemap();
            var parentNodeLookup = allpages.ToLookup(x => x.ParentNodeId);
            PopulateChildNodes(sitemap, parentNodeLookup);

            return sitemap;
        }

        private static void PopulateChildNodes(ISitemapNode sitemap, ILookup<Guid, SitemapNode> parentNodeLookup)
        {
            sitemap.ChildNodes = parentNodeLookup[sitemap.NodeId].ToList();

            foreach (var childNode in sitemap.ChildNodes)
                PopulateChildNodes(childNode, parentNodeLookup);

        }
    }


}
