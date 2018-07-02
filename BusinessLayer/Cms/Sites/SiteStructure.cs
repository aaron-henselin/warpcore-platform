using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms.Sites
{
    public interface ISiteStructureNode
    {
        Guid NodeId { get; }
        IReadOnlyCollection<CmsPageLocationNode> ChildNodes { get; set; }
    }

    public class SiteStructure : ISiteStructureNode
    {
        public Guid NodeId => Guid.Empty;
        public IReadOnlyCollection<CmsPageLocationNode> ChildNodes { get; set; } = new List<CmsPageLocationNode>();


        public ISiteStructureNode GetStructureNode(CmsPage cmsPage)
        {
            return GetStructureNode(cmsPage, this);
        }

        private static ISiteStructureNode GetStructureNode(CmsPage cmsPage, ISiteStructureNode root)
        {
            foreach (var childNode in root.ChildNodes)
            {
                if (childNode.PageId == cmsPage.ContentId)
                    return childNode;

                var found = GetStructureNode(cmsPage, childNode);
                if (found != null)
                    return found;
            }

            return null;
        }

    }



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
                ll.Insert(0, remainingRoot);
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

}
