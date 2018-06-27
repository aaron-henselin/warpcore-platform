using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using WarpCore.Cms.Routing;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public struct PageType
    {
        public const string ContentPage="ContentPage";
        public const string GroupingPage = "GroupingPage";
        public const string RedirectPage = "RedirectPage";
    }

    [Table("cms_page")]
    public class CmsPage:VersionedContentEntity
    {



        [Column]
        public string Name { get; set; }

        [Column]
        public string Slug { get; set; }

        [Column]
        public Guid LayoutId { get; set; }


        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public string PageType { get; set; }

        [ComplexData]
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();

        [ComplexData]
        public List<PageRoute> AlternateRoutes { get; set; } = new List<PageRoute>();

        public string PhysicalFile { get; set; }
        public string RedirectExternalUrl { get; set; }

        public Guid? RedirectPageId { get; set; }

        [Column]
        public int Order { get; set; }

        [Column]
        public bool RequireSsl { get; set; }
    }

    public enum RoutePriority
    {
        Primary = 0,
        Former = 10,
    }

    [Table("cms_site_route")]
    public class PageRoute : CosmosEntity
    {
        [Column]
        public Guid PageId { get; set; }

        [Column]
        public string VirtualPath { get; set; }

        [Column]
        public int Priority { get; set; }

        [Column]
        public int Order { get; set; }
    }




    [Table("cms_page_content")]
    public class CmsPageContent :CosmosEntity
    {
        [Column]
        public string ContentPlaceHolderId { get; set; }

        [Column]
        public string WidgetTypeCode { get; set; }

        [Column]
        public Dictionary<string,string> Parameters { get; set; }
    }

    public class DuplicateSlugException:Exception
    {
    }

    public class SitemapRelativePosition
    {
        public Guid? ParentSitemapNodeId { get; set; }
        public Guid? BeforeSitemapNodeId { get; set; }

        public static SitemapRelativePosition Root => new SitemapRelativePosition{ParentSitemapNodeId = Guid.Empty};
    }

    public class PageRelativePosition
    {
        public Guid? ParentPageId { get; set; }
        public Guid? BeforePageId { get; set; }
    }

    public class PageRepository : VersionedContentRepository<CmsPage>
    {


        public string CreateVirtualPath(CmsPage cmsPage)
        {

            throw new NotImplementedException();

            //var generated = SlugGenerator.Generate(cmsPage.Name);
            //if (cmsPage.ParentPageId != null)
            //    return CreateVirtualPath(cmsPage) + "/" + generated;

            //return "/"+generated;
        }

        private void AssertSlugIsNotTaken(CmsPage cmsPage, SitemapRelativePosition newSitemapRelativePosition)
        {
            ISiteStructureNode parentNode;
            if (Guid.Empty == newSitemapRelativePosition.ParentSitemapNodeId)
                parentNode = new SiteStructure();
            else
            {
                var findParentCondition = $@"{nameof(SiteStructureNode.ContentId)} eq '{newSitemapRelativePosition.ParentSitemapNodeId}'";
                var parentNodes = Orm.FindUnversionedContent<SiteStructureNode>(findParentCondition).Result;
                if (!parentNodes.Any())
                    throw new Exception("Could not find a structual node for: '" + findParentCondition + "'");

                parentNode = parentNodes.Single();
            }

            var siblingsCondition = $@"{nameof(SiteStructureNode.PageId)} neq '{cmsPage.ContentEnvironment}' and {nameof(SiteStructureNode.ParentNodeId)} eq '{parentNode.NodeId}'";
            var siblings = Orm.FindUnversionedContent<SiteStructureNode>(siblingsCondition).Result.ToList();


            //foreach (var sibling in siblings)
            //{
            //    this.FindContentVersions()
            //}



            //var dupSlugs = GetAllPages()
            //    .Where(x => x.ParentPageId == cmsPage.ParentPageId && x.Id != cmsPage.Id)
            //    .SelectMany(x => x.Routes)
            //    .Where(x => x.Priority == (int) RoutePriority.Primary);

            //if (dupSlugs.Any())
            //    throw new DuplicateSlugException();
        }

        public void Move(CmsPage page, SitemapRelativePosition newSitemapRelativePosition)
        {
            var condition = $@"{nameof(SiteStructureNode.PageId)} eq '{page.ContentId}'";
            var sitemapNode = Orm.FindUnversionedContent<SiteStructureNode>(condition).Result.SingleOrDefault();
            if (sitemapNode == null)
                sitemapNode = new SiteStructureNode();

            sitemapNode.ContentId = Guid.NewGuid();
            sitemapNode.PageId = page.ContentId.Value;
            sitemapNode.SiteId = page.SiteId;
            sitemapNode.ParentNodeId = newSitemapRelativePosition.ParentSitemapNodeId.Value;
            sitemapNode.BeforeNodeId = newSitemapRelativePosition.BeforeSitemapNodeId;

            var sitemapNodesToUpdate = Orm.FindUnversionedContent<SiteStructureNode>($"ParentNodeId eq '{newSitemapRelativePosition.ParentSitemapNodeId.Value}'").Result;
            var previousBefore = sitemapNodesToUpdate.SingleOrDefault(x => x.BeforeNodeId == newSitemapRelativePosition.BeforeSitemapNodeId);

            Orm.Save(sitemapNode);
            if (previousBefore != null)
            {
                previousBefore.BeforeNodeId = sitemapNode.ContentId;
                Orm.Save(previousBefore);
            }
           
        }

        public void Save(CmsPage cmsPage, PageRelativePosition pageRelativePosition)
        {
            var position = new SitemapRelativePosition
            {
                
            };

            if (pageRelativePosition.ParentPageId != null)
            {
                var parentNodeSearch = $@"{nameof(SiteStructureNode.PageId)} eq '{pageRelativePosition.ParentPageId}'";
                var parentNode = Orm.FindUnversionedContent<SiteStructureNode>(parentNodeSearch).Result
                    .SingleOrDefault();

                position.ParentSitemapNodeId = parentNode?.NodeId;
            }


            if (pageRelativePosition.BeforePageId != null)
            {
                var beforeNodeSearch = $@"{nameof(SiteStructureNode.PageId)} eq '{pageRelativePosition.BeforePageId}'";
                var beforeNode = Orm.FindUnversionedContent<SiteStructureNode>(beforeNodeSearch).Result
                    .SingleOrDefault();

                position.BeforeSitemapNodeId = beforeNode?.NodeId;
            }


            Save(cmsPage, position);

        }

        public void Save(CmsPage cmsPage, SitemapRelativePosition newSitemapRelativePosition)
        {
            if (cmsPage.SiteId == default(Guid))
                throw new Exception("Must specify site.");

            if (string.IsNullOrWhiteSpace(cmsPage.Slug))
            {
                GenerateUniqueSlugForPage(cmsPage, newSitemapRelativePosition);
            }
            else
                AssertSlugIsNotTaken(cmsPage, newSitemapRelativePosition);

            base.Save(cmsPage);

            Move(cmsPage,newSitemapRelativePosition);
        }

        public override void Save(CmsPage cmsPage)
        {
            SitemapRelativePosition sitemapRelativePosition;
            if (cmsPage.IsNew)
                sitemapRelativePosition = SitemapRelativePosition.Root;
            else
            {
                var condition = $@"{nameof(SiteStructureNode.PageId)} eq '{cmsPage.ContentId}'";
                var node = Orm.FindUnversionedContent<SiteStructureNode>(condition).Result.Single();
                sitemapRelativePosition = new SitemapRelativePosition
                {
                    ParentSitemapNodeId = node.ParentNodeId,
                    BeforeSitemapNodeId = node.BeforeNodeId,
                };
            }


            Save(cmsPage, sitemapRelativePosition);


            //todo: save this stuff.
            //page.Routes.Where(x => x.Slug == newDefaultSlug)

            //_dbAdapter.Save();
            //SlugGenerator.Generate(page.Name)
            //new RouteRepository().GetAllRoutes().
            //new Page().Name
            //page.Name
        }

        private void GenerateUniqueSlugForPage(CmsPage cmsPage, SitemapRelativePosition sitemapRelativePosition)
        {
            if (string.IsNullOrWhiteSpace(cmsPage.Name))
                throw new Exception("Page name is required.");

            bool foundUniqueSlug = false;
            cmsPage.Slug = SlugGenerator.Generate(cmsPage.Name);

            int counter = 2;
            while (!foundUniqueSlug)
            {
                var rawSlug = cmsPage.Name;
                try
                {
                    AssertSlugIsNotTaken(cmsPage, sitemapRelativePosition);
                    foundUniqueSlug = true;
                }
                catch (DuplicateSlugException e)
                {
                    cmsPage.Name = rawSlug + counter;
                    counter++;
                }
            }
        }
    }

}
