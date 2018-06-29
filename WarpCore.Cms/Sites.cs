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
        public static SiteStructure BuildStructureMap(Site site)
        {
            var sitemapLookup = $"{nameof(CmsPageLocationNode.SiteId)} eq '{site.ContentId}'";
            var allpages = Dependency.Resolve<ICosmosOrm>().FindUnversionedContent<CmsPageLocationNode>(sitemapLookup).Result.ToList();

            var sitemap = new SiteStructure();
            var parentNodeLookup = allpages.ToLookup(x => x.ParentNodeId);
            PopulateChildNodes(sitemap, parentNodeLookup);

            return sitemap;
        }

        private static void PopulateChildNodes(ISiteStructureNode node, ILookup<Guid, CmsPageLocationNode> parentNodeLookup)
        {
            var ll = new List<CmsPageLocationNode>();

            var pendingToInsert = parentNodeLookup[node.NodeId].ToList();

            if (pendingToInsert.Any())
            {
                var remainingRoot = pendingToInsert.Single(x => x.BeforeNodeId == null);
                pendingToInsert.Remove(remainingRoot);
                ll.Insert(0,remainingRoot);
            }

            while (pendingToInsert.Any())
            {
                var nextBefore = pendingToInsert.SingleOrDefault(x => x.BeforeNodeId == ll.First().NodeId);
                pendingToInsert.Remove(nextBefore);
                ll.Insert(0, nextBefore);
            }

            node.ChildNodes = ll;

            foreach (var childNode in node.ChildNodes)
                PopulateChildNodes(childNode, parentNodeLookup);

        }
    }

    public class SiteRepository : UnversionedContentRepository<Site>
    {



    }


}
