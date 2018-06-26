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
    public class Site : UnversionedContentEntity
    {
        public string Name { get; set; }
        public string RoutePrefix { get; set; }
        public string UriAuthority { get; set; }
        public int Priority { get; set; }

        public Guid? HomepageId { get; set; }
    }

    public class SiteStructureMapBuilder
    {
        public SiteStructure BuildStructureMap(Site site)
        {
            var sitemapLookup = $"{nameof(SiteStructureNode.SiteId)} eq '{site.ContentId}'";
            var allpages = Dependency.Resolve<ICosmosOrm>().FindUnversionedContent<SiteStructureNode>(sitemapLookup).Result.ToList();

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

    public class SiteRepository : UnversionedContentRepository<Site>
    {



    }


}
