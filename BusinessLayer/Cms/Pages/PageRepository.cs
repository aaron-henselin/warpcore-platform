using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public struct PageType
    {
        public const string ContentPage = "ContentPage";
        public const string GroupingPage = "GroupingPage";
        public const string RedirectPage = "RedirectPage";
    }

    [Table("cms_page")]
    public class CmsPage : VersionedContentEntity
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
        public string PageType { get; set; } = WarpCore.Cms.PageType.ContentPage;

        [StoreAsComplexData]
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();

        //[ComplexData]
        //public List<HistoricalRoute> AlternateRoutes { get; set; } = new List<HistoricalRoute>();

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


    public interface IPageContent
    {
        List<CmsPageContent> SubContent { get; } 
    }

    public class FoundSubContent
    {
        public CmsPageContent LocatedContent { get; set; }
        public IPageContent ParentContent { get; set; }
    }

    public static class IPageContentExtensions
    {

        public static IReadOnlyCollection<FoundSubContent> FindSubContentReursive(this IPageContent pageContent, Func<CmsPageContent, bool> match)
        {
            List<FoundSubContent> foundContents = new List<FoundSubContent>();
            foreach (var originalItem in pageContent.SubContent)
            {
                if (match(originalItem))
                    foundContents.Add(new FoundSubContent{ParentContent = pageContent,LocatedContent = originalItem});

                var additionalResults = FindSubContentReursive(originalItem,match);
                foundContents.AddRange(additionalResults);
            }
            return foundContents;
        }

        public static void RemoveSubContentReursive(this IPageContent pageContent, Func<CmsPageContent, bool> match)
        {
            var originalItems = pageContent.SubContent.ToList();
            foreach (var originalItem in originalItems)
            {
                if (match(originalItem))
                    pageContent.SubContent.Remove(originalItem);
            }

            foreach (var remainingSubContentItem in pageContent.SubContent)
                RemoveSubContentReursive(remainingSubContentItem,match);
        }
    }



    public class CmsPageContent : IPageContent
    {
        [Column]
        public Guid Id { get; set; }

        [Column]
        public string PlacementContentPlaceHolderId { get; set; }

        [Column]
        public Guid? PlacementLayoutBuilderId { get; set; }

        [Column]
        public int Order { get; set; }

        [Column]
        public string WidgetTypeCode { get; set; }

        [Column]
        public Dictionary<string,string> Parameters { get; set; }

        [StoreAsComplexData]
        public List<CmsPageContent> SubContent { get; set; } = new List<CmsPageContent>();




        //public List<CmsPageContent> FindOwningPageContentCollection(Guid id)
        //{
        //    foreach (var content in LocatedContent)
        //    {
        //        if (content.Id == id)
        //            return LocatedContent;

        //        var result = content.FindOwningPageContentCollection(id);
        //        if (result != null)
        //            return result;
        //    }

        //    return null;
        //}
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

    [Table("cms_site_route")]
    public class HistoricalRoute : UnversionedContentEntity
    {
        [Column]
        public Guid PageId { get; set; }

        [Column]
        public string VirtualPath { get; set; }

        [Column]
        public int Priority { get; set; }

        [Column]
        public int Order { get; set; }

        public Guid SiteId { get; set; }
    }



    public class PageRepository : VersionedContentRepository<CmsPage>
    {


        private void AssertSlugIsNotTaken(CmsPage cmsPage, SitemapRelativePosition newSitemapRelativePosition)
        {
            ISiteStructureNode parentNode;
            if (Guid.Empty == newSitemapRelativePosition.ParentSitemapNodeId)
                parentNode = new SiteStructure();
            else
            {
                var findParentCondition = $@"{nameof(CmsPageLocationNode.ContentId)} eq '{newSitemapRelativePosition.ParentSitemapNodeId}'";
                var parentNodes = Orm.FindUnversionedContent<CmsPageLocationNode>(findParentCondition).Result;
                if (!parentNodes.Any())
                    throw new Exception("Could not find a structual node for: '" + findParentCondition + "'");

                parentNode = parentNodes.Single();
            }

            var siblingsCondition = $@"{nameof(CmsPageLocationNode.PageId)} neq '{cmsPage.ContentEnvironment}' and {nameof(CmsPageLocationNode.ParentNodeId)} eq '{parentNode.NodeId}'";
            var siblings = Orm.FindUnversionedContent<CmsPageLocationNode>(siblingsCondition).Result.ToList();


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

            var condition = $@"{nameof(CmsPageLocationNode.PageId)} eq '{page.ContentId}'";
            var newPageLocation = Orm.FindUnversionedContent<CmsPageLocationNode>(condition).Result.SingleOrDefault();
            if (newPageLocation == null)
                newPageLocation = new CmsPageLocationNode();
            else
            {
                AppendToRouteHistory(page);
            }

            newPageLocation.ContentId = Guid.NewGuid();
            newPageLocation.PageId = page.ContentId.Value;
            newPageLocation.SiteId = page.SiteId;
            newPageLocation.ParentNodeId = newSitemapRelativePosition.ParentSitemapNodeId.Value;
            newPageLocation.BeforeNodeId = newSitemapRelativePosition.BeforeSitemapNodeId;

            var sitemapNodesToUpdate = Orm.FindUnversionedContent<CmsPageLocationNode>($"SiteId eq '{page.SiteId}' and ParentNodeId eq '{newSitemapRelativePosition.ParentSitemapNodeId.Value}'").Result;
            var previousBefore = sitemapNodesToUpdate.SingleOrDefault(x => x.BeforeNodeId == newSitemapRelativePosition.BeforeSitemapNodeId);

            Orm.Save(newPageLocation);
            if (previousBefore != null)
            {
                previousBefore.BeforeNodeId = newPageLocation.ContentId;
                Orm.Save(previousBefore);
            }

            //this addresses only half of the linked list move.
            //need to relink 

        }

        public IEnumerable<HistoricalRoute> GetHistoricalPageLocations(Site site)
        {
            return Orm.FindUnversionedContent<HistoricalRoute>("SiteId eq '" + site.ContentId + "'").Result;
        }


        private void AppendToRouteHistory(CmsPage page)
        {
            var site = new SiteRepository().GetById(page.SiteId);
            var sitemap = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live);
            var previousLivePosition = sitemap.GetSitemapNode(page);
            if (previousLivePosition != null)
            {
                var historicalRoute = new HistoricalRoute
                {
                    Priority = (int) RoutePriority.Former,
                    VirtualPath = previousLivePosition.VirtualPath.ToString(),
                    PageId = page.ContentId.Value,
                    SiteId = page.SiteId,
                };
                Orm.Save(historicalRoute);
            }
        }

        public void Save(CmsPage cmsPage, PageRelativePosition pageRelativePosition)
        {
            var position = new SitemapRelativePosition
            {
                
            };

            if (pageRelativePosition.ParentPageId != null)
            {
                var parentNodeSearch = $@"{nameof(CmsPageLocationNode.PageId)} eq '{pageRelativePosition.ParentPageId}'";
                var parentNode = Orm.FindUnversionedContent<CmsPageLocationNode>(parentNodeSearch).Result
                    .SingleOrDefault();

                position.ParentSitemapNodeId = parentNode?.NodeId;
            }


            if (pageRelativePosition.BeforePageId != null)
            {
                var beforeNodeSearch = $@"{nameof(CmsPageLocationNode.PageId)} eq '{pageRelativePosition.BeforePageId}'";
                var beforeNode = Orm.FindUnversionedContent<CmsPageLocationNode>(beforeNodeSearch).Result
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
                var condition = $@"{nameof(CmsPageLocationNode.PageId)} eq '{cmsPage.ContentId}'";
                var node = Orm.FindUnversionedContent<CmsPageLocationNode>(condition).Result.Single();
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
