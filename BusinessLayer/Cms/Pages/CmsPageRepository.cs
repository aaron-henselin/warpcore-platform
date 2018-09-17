using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using Cms;
using Cms.Toolbox;
using DemoSite;
using Framework;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{
    public interface IHasDesignedLayout
    {
        Guid DesignForContentId { get; }

        List<CmsPageContent> DesignedContent { get; }
    }


    public struct PageType
    {
        public const string ContentPage = "ContentPage";
        public const string GroupingPage = "GroupingPage";
        public const string RedirectPage = "RedirectPage";
    }
    
    [Table("cms_page")]
    [SupportsCustomFields(TypeResolverUid)]
    [GroupUnderParentRepository(CmsPageRepository.ApiId)]
    [ContentDescription(ContentFriendlyNameSingular = "Page")]
    public class CmsPage : VersionedContentEntity, IHasDesignedLayout
    {
        public const string TypeResolverUid = "5299865c-8c7c-47e2-8ca0-d7615dde8377";

        [Column]
        public string Name { get; set; }

        [Column]
        public string Slug { get; set; }

        [Column]
        public Guid LayoutId { get; set; }

        [Column]
        public string Keywords { get; set; }

        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public string PageType { get; set; } = WarpCore.Cms.PageType.ContentPage;

        [SerializedComplexObject]
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();

        public bool DisplayInNavigation { get; set; } = true;

        //[ComplexData]
        //public List<HistoricalRoute> AlternateRoutes { get; set; } = new List<HistoricalRoute>();

        public string PhysicalFile { get; set; }
        public string RedirectExternalUrl { get; set; }

        public Guid? RedirectPageId { get; set; }

        [Column]
        public bool RequireSsl { get; set; }

        public List<CmsPageContent> DesignedContent => PageContent;
        public Guid DesignForContentId => ContentId;

        [Column]
        public bool EnableViewState { get; set; }

        [Column]
        public string Description { get; set; }

        [SerializedComplexObject]
        public Dictionary<string, string> InternalRedirectParameters { get; set; }
    }

    public enum RoutePriority
    {
        Primary = 0,
        Former = 10,
    }


    public interface IPageContent
    {
        List<CmsPageContent> AllContent { get; } 
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
            foreach (var originalItem in pageContent.AllContent)
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
            var originalItems = pageContent.AllContent.ToList();
            foreach (var originalItem in originalItems)
            {
                if (match(originalItem))
                    pageContent.AllContent.Remove(originalItem);
            }

            foreach (var remainingSubContentItem in pageContent.AllContent)
                RemoveSubContentReursive(remainingSubContentItem,match);
        }
    }

    public class CmsPageContentFactory
    {
        public CmsPageContent CreateToolboxItemContent<T>(T activated=null) where T : Control
        {
            var toolboxMetadata = ToolboxMetadataReader.ReadMetadata(typeof(T));
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(toolboxMetadata.WidgetUid);

            IDictionary<string, string> settings;

            if (activated != null)
                settings = activated.GetPropertyValues(ToolboxPropertyFilter.IsConfigurable);
            else
                settings = CmsPageContentActivator.GetDefaultContentParameterValues(toolboxItem);

            return new CmsPageContent(toolboxItem, settings);
        }
    }


    public class CmsPageContent : IPageContent
    {
        public CmsPageContent()
        {
        }

        public CmsPageContent(ToolboxItem toolboxItem, IDictionary<string,string> settings)
        {
            Id = Guid.NewGuid();
            WidgetTypeCode = toolboxItem.WidgetUid;
            Parameters = settings.ToDictionary(x => x.Key,x => x.Value);
        }

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

        [SerializedComplexObject]
        public Dictionary<string,string> Parameters { get; set; }

        [SerializedComplexObject]
        public List<CmsPageContent> AllContent { get; set; } = new List<CmsPageContent>();




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

    public class GroupUnderParentRepositoryAttribute :Attribute
    {
        public GroupUnderParentRepositoryAttribute(string parentRepositoryApiId)
        {
            ParentRepositoryId = new Guid(parentRepositoryApiId);
        }

        public Guid ParentRepositoryId { get; set; }
    }

    public class ContentDescriptionAttribute : Attribute
    {
        public string ContentFriendlyNameSingular { get; set; }
        public string ContentFriendlyNamePlural { get; set; }
    }

    public class SupportsCustomFieldsAttribute : Attribute
    {
        public SupportsCustomFieldsAttribute(string typeExtensionId)
        {
            TypeExtensionUid = new Guid(typeExtensionId);
        }

        public Guid TypeExtensionUid { get; set; }
    }

    public class ExposeToWarpCoreApi : Attribute
    {
        public ExposeToWarpCoreApi(string uid)
        {
            TypeUid = uid;
        }

        public string TypeUid { get; set; }
    }




    [ExposeToWarpCoreApi(ApiId)]
    public class CmsPageRepository : VersionedContentRepository<CmsPage>
    {
        public const string ApiId = "979fde2a-1983-480e-aca4-8caab3f762b0";

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
            newPageLocation.PageId = page.ContentId;
            newPageLocation.SiteId = page.SiteId;
            newPageLocation.ParentNodeId = newSitemapRelativePosition.ParentSitemapNodeId.Value;
            //newPageLocation.BeforeNodeId = newSitemapRelativePosition.BeforeSitemapNodeId;


            var sitemapNodesToUpdate = Orm.FindUnversionedContent<CmsPageLocationNode>($"SiteId eq '{page.SiteId}' and ParentNodeId eq '{newSitemapRelativePosition.ParentSitemapNodeId.Value}'").Result;
            var collection = sitemapNodesToUpdate.ToList();

            var insertAt = 0;
            if (null != newSitemapRelativePosition.BeforeSitemapNodeId)
            {
                var beforeNode = collection.Single(x => newSitemapRelativePosition.BeforeSitemapNodeId == x.NodeId);
                insertAt = collection.IndexOf(beforeNode);
            }
            collection.Insert(insertAt,newPageLocation);

            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].Order = i;
                Orm.Save(collection[i]);
            }

            CmsRoutes.RegenerateAllRoutes();

            //var previousBefore = sitemapNodesToUpdate.SingleOrDefault(x => x.BeforeNodeId == newSitemapRelativePosition.BeforeSitemapNodeId);

            //Orm.Save(newPageLocation);
            //if (previousBefore != null)
            //{
            //    previousBefore.BeforeNodeId = newPageLocation.ContentId;
            //    Orm.Save(previousBefore);
            //}

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
            var sitemap = SitemapBuilder.BuildSitemap(site, ContentEnvironment.Live, SitemapBuilderFilters.All);
            var previousLivePosition = sitemap.GetSitemapNode(page);
            if (previousLivePosition != null)
            {
                var historicalRoute = new HistoricalRoute
                {
                    Priority = (int) RoutePriority.Former,
                    VirtualPath = previousLivePosition.VirtualPath.ToString(),
                    PageId = page.ContentId,
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
            {
                var defaultFrontendSite = SiteManagementContext.GetSiteToManage();
                if (defaultFrontendSite != null)
                    cmsPage.SiteId = defaultFrontendSite.ContentId;
                else
                throw new Exception("Must specify site.");
            }

            if (string.IsNullOrWhiteSpace(cmsPage.Slug))
            {
                GenerateUniqueSlugForPage(cmsPage, newSitemapRelativePosition);
            }
            else
                AssertSlugIsNotTaken(cmsPage, newSitemapRelativePosition);

            base.SaveImpl(cmsPage);

            Move(cmsPage,newSitemapRelativePosition);
        }

        
        protected override void SaveImpl(VersionedContentEntity vce)
        {
            CmsPage cmsPage = (CmsPage) vce;

            SitemapRelativePosition sitemapRelativePosition;
            if (cmsPage.IsNew)
                sitemapRelativePosition = SitemapRelativePosition.Root;
            else
            {
                var existingLocationSearch = $@"{nameof(CmsPageLocationNode.PageId)} eq '{cmsPage.ContentId}'";
                var node = Orm.FindUnversionedContent<CmsPageLocationNode>(existingLocationSearch).Result.Single();


                var siblingSearch = $@"{nameof(CmsPageLocationNode.SiteId)} eq '{cmsPage.SiteId}' and {nameof(CmsPageLocationNode.ParentNodeId)} eq '{node.ParentNodeId}'";
                var siblingNodes = Orm.FindUnversionedContent<CmsPageLocationNode>(siblingSearch).Result;
                var newBeforeNode = siblingNodes.Where(x => x.Order > node.Order).OrderBy(x => x.Order).FirstOrDefault();


                sitemapRelativePosition = new SitemapRelativePosition
                {
                    ParentSitemapNodeId = node.ParentNodeId,
                    BeforeSitemapNodeId = newBeforeNode?.NodeId,
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
