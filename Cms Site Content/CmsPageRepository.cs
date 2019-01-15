using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Cms.Layout;
using Platform_Security;
using WarpCore.Cms.Sites;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace WarpCore.Cms
{
    public class SiteStructureChanged : IDomainEvent
    {
    }

    public interface IHasSubRenderingPlans
    {
        Guid DesignForContentId { get; }
        Guid ContentTypeId { get; }
        List<CmsPageContent> DesignedContent { get; }
    }


    public struct PageType
    {
        public const string ContentPage = "ContentPage";
        public const string GroupingPage = "GroupingPage";
        public const string RedirectPage = "RedirectPage";
    }





    [Table("cms_page")]
    [WarpCoreEntity(ApiId,TitleProperty =nameof(Name),ContentNameSingular = "Page")]
    [GroupUnderParentRepository(CmsPageRepository.ApiId)]
    public class CmsPage : VersionedContentEntity, IHasSubRenderingPlans
    {
        public const string ApiId = "5299865c-8c7c-47e2-8ca0-d7615dde8377";

        [Column]
        public string Name { get; set; }

        [Column]
        public string Slug { get; set; }

        [Column][DisplayName("Layout")]
        [DataRelation(LayoutRepository.ApiId)]
        public Guid LayoutId { get; set; }

        [Column]
        public string Keywords { get; set; }

        [Column]
        [DisplayName("Site")]
        [DataRelation(SiteRepository.ApiId)]
        public Guid SiteId { get; set; }

        [Column]
        [DisplayName("Page Type")]
        public string PageType { get; set; } = WarpCore.Cms.PageType.ContentPage;

        [SerializedComplexObject]
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();

        public bool DisplayInNavigation { get; set; } = true;

        //[ComplexData]
        //public List<HistoricalRoute> AlternateRoutes { get; set; } = new List<HistoricalRoute>();

        [DisplayName("Physical File")]
        public string PhysicalFile { get; set; }


        [DisplayName("Redirect Url")]
        public Uri RedirectUri { get; set; }

        [Column][DisplayName("Require Ssl")]
        public bool RequireSsl { get; set; }

        public Guid ContentTypeId => new Guid(CmsPageRepository.ApiId);

        public List<CmsPageContent> DesignedContent => PageContent;
        public Guid DesignForContentId => ContentId;

        [Column][DisplayName("Enable ViewState")]
        public bool EnableViewState { get; set; }

        [Column]
        public string Description { get; set; }

        [SerializedComplexObject]
        public Dictionary<string, string> InternalRedirectParameters { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public CmsPageContent FindContentById(Guid id)
        {
            foreach (var child in this.PageContent)
            {
                var found = FindContentById(child, id);
                if (found != null)
                    return found;
            }

            return null;
        }

        private CmsPageContent FindContentById(CmsPageContent searchNode, Guid id)
        {
            if (searchNode.Id == id)
                return searchNode;

            foreach (var child in searchNode.AllContent)
            {
                var found = FindContentById(child, id);
                if (found != null)
                    return found;
            }

            return null;
        }
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

    public static class KnownFormIds
    {
        public static Guid ContentPageSettingsForm => new Guid("7f85cee1-9ce8-463d-b0a2-9ca93e09608d");
        public static Guid GroupingPageSettingsForm => new Guid("ac9151e9-febf-4787-8291-5c0bfa2f0b0f");
        public static Guid RedirectPageSettingsForm => new Guid("922f3485-3b5b-4087-ab4d-29765e638042");
    }



    public static class KnownPageIds
    {

        public static Guid AddPageWizard => new Guid("f05448d0-bb10-4de7-a5db-a1d85bbb459f");
        public static Guid ContentPageSettings => new Guid("f7f2332e-d2eb-4202-8e9f-99e0b1644386");
        public static Guid GroupingPageSettings => new Guid("e35ac840-0bf5-47ad-b067-0ce096efa9d5");
        public static Guid RedirectPageSettings => new Guid("aeaaa389-b57a-4885-ba74-50daf1411973");

        


        public static Guid FormDesigner => new Guid("f6677c22-249a-4fe0-8027-6153c3bf7356");



    }


    public class CmsPageContent : IPageContent
    {
        public CmsPageContent()
        {
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




    public class DataRelationAttribute : Attribute
    {
        public string ApiId { get; set; }

        public DataRelationAttribute(string apiId)
        {
            ApiId = apiId;
        }


    }



    //public class ContentSecurityModel : IRepositorySecurityModel
    //{
    //    public const string ModelName = "Content";

    //    public PermissionRuleSet CalculatePermissions(SecurityQuery securedResourceId)
    //    {
    //        var allPermissions = new PermissionRepository().Find().Where(x => x.SecurityModel == ModelName).ToLookup(x => x.SecuredResourceId);

    //        securedResourceId.RepositoryApiId
    //    }
    //}

    public class SitemapSecurityModel : IRepositorySecurityModel
    {
        public const string ModelName = "Sitemap";

        private static Dictionary<Guid, PermissionRuleSet> CalculatePermissionsForAllSites()
        {
            Dictionary<Guid, PermissionRuleSet> newRules = new Dictionary<Guid, PermissionRuleSet>();
            var allPermissions = new PermissionRepository().Find().Where(x => x.SecurityModel == ModelName).ToLookup(x => x.SecuredResourceId);

            var siteRepository = new SiteRepository();
            foreach (var site in siteRepository.Find())
            {
                var subDict = CalculatePermissionsForSite(site, allPermissions);
                foreach (var kvp in subDict)
                    newRules.Add(kvp.Key,kvp.Value);
            }

            return newRules;
        }

        private static Dictionary<Guid, PermissionRuleSet> CalculatePermissionsForSite(Site site, ILookup<Guid, PermissionRule> allPermissions)
        {
            PermissionRuleSet defaultPermissions;
            if (site.IsFrontendSite)
            {
                defaultPermissions = new PermissionRuleSet();
                var readRule = new PermissionRule
                {
                    AppliesToRoleName = "Everyone",
                    PermissionType = PermissionType.Grant,
                    PrivilegeName = KnownPrivilegeNames.Read
                };
                var createRule = new PermissionRule
                {
                    AppliesToRoleName = "Everyone",
                    PermissionType = PermissionType.Deny,
                    PrivilegeName = KnownPrivilegeNames.Create
                };
                var updateRule = new PermissionRule
                {
                    AppliesToRoleName = "Everyone",
                    PermissionType = PermissionType.Deny,
                    PrivilegeName = KnownPrivilegeNames.Update
                };
                var deleteRule = new PermissionRule
                {
                    AppliesToRoleName = "Everyone",
                    PermissionType = PermissionType.Deny,
                    PrivilegeName = KnownPrivilegeNames.Delete
                };

                defaultPermissions.Add(readRule);
                defaultPermissions.Add(createRule);
                defaultPermissions.Add(updateRule);
                defaultPermissions.Add(deleteRule);
            }
            else
            {
                defaultPermissions = new PermissionRuleSet();
                var readRule = new PermissionRule
                {
                    AppliesToRoleName = "BackendUsers",
                    PermissionType = PermissionType.Grant,
                    PrivilegeName = KnownPrivilegeNames.Read
                };
                var createRule = new PermissionRule
                {
                    AppliesToRoleName = "BackendUsers",
                    PermissionType = PermissionType.Grant,
                    PrivilegeName = KnownPrivilegeNames.Create
                };
                var updateRule = new PermissionRule
                {
                    AppliesToRoleName = "BackendUsers",
                    PermissionType = PermissionType.Grant,
                    PrivilegeName = KnownPrivilegeNames.Update
                };
                var deleteRule = new PermissionRule
                {
                    AppliesToRoleName = "BackendUsers",
                    PermissionType = PermissionType.Grant,
                    PrivilegeName = KnownPrivilegeNames.Delete
                };


                defaultPermissions.Add(readRule);
                defaultPermissions.Add(createRule);
                defaultPermissions.Add(updateRule);
                defaultPermissions.Add(deleteRule);
            }

            var siteStructure = SiteStructureMapBuilder.BuildStructureMap(site);

            Dictionary<Guid, PermissionRuleSet> newRules = new Dictionary<Guid, PermissionRuleSet>();
            foreach (var page in siteStructure.ChildNodes)
            {
                var subDict = CalculatePermissions(page, allPermissions, defaultPermissions);
                foreach (var kvp in subDict)
                    newRules.Add(kvp.Key, kvp.Value);
            }
            return newRules;
        }

        private static Dictionary<Guid, PermissionRuleSet> CalculatePermissions(CmsPageLocationNode siteStructure, ILookup<Guid, PermissionRule> allPermissions, PermissionRuleSet defaultPermissions)
        {
            Dictionary<Guid, PermissionRuleSet> newRules = new Dictionary<Guid, PermissionRuleSet>();

            var permissionsForPage = allPermissions[siteStructure.PageId].ToList();
            if (!permissionsForPage.Any())
                newRules.Add(siteStructure.PageId, new PermissionRuleSet(defaultPermissions));
            else
            {
                newRules.Add(siteStructure.PageId, new PermissionRuleSet(permissionsForPage));
            }

            foreach (var childNode in siteStructure.ChildNodes)
            {
                var subDict = CalculatePermissions(childNode, allPermissions, newRules[siteStructure.PageId]);
                foreach (var kvp in subDict)
                    newRules.Add(kvp.Key, kvp.Value);
            }
            return newRules;
        }

        object syncRoot = new object();
        private static Dictionary<Guid, PermissionRuleSet> _allRules;



        public PermissionRuleSet CalculatePermissions(SecurityQuery query)
        {
            lock (syncRoot)
            {
                //if (_allRules == null)
                var _allRules = CalculatePermissionsForAllSites();
                if (!_allRules.ContainsKey(query.ItemId))
                    throw new Exception("Security was not calculated for secured resource id: " + query.ItemId);

                return _allRules[query.ItemId];
            }
        }
    }



    [ExposeToWarpCoreApi(ApiId)]
    public class CmsPageRepository : VersionedContentRepository<CmsPage>
    {
        public const string ApiId = "979fde2a-1983-480e-aca4-8caab3f762b0";

        protected override IRepositorySecurityModel SecurityModel { get; } = new SitemapSecurityModel();

        private void AssertSlugIsNotTaken(CmsPage cmsPage, SitemapRelativePosition newSitemapRelativePosition)
        {
            ISiteStructureNode parentNode;
            if (Guid.Empty == newSitemapRelativePosition.ParentSitemapNodeId)
                parentNode = new SiteStructure(cmsPage.SiteId);
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

            if (newSitemapRelativePosition.ParentSitemapNodeId == null ||
                newSitemapRelativePosition.ParentSitemapNodeId == Guid.Empty)
                newSitemapRelativePosition.ParentSitemapNodeId = page.SiteId;

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

            DomainEvents.Raise(new SiteStructureChanged());



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

        private void EnsureIdsAssigned(CmsPageContent content)
        {
            if (content.Id == Guid.Empty)
                content.Id = Guid.NewGuid();

            foreach (var contentItem in content.AllContent)
                EnsureIdsAssigned(contentItem);
        }

        public void Save(CmsPage cmsPage, SitemapRelativePosition newSitemapRelativePosition)
        {
            foreach (var content in cmsPage.DesignedContent)
                EnsureIdsAssigned(content);

            if (cmsPage.SiteId == default(Guid))
            {
                //var defaultFrontendSite = SiteManagementContext.GetSiteToManage();
                //if (defaultFrontendSite != null)
                //    cmsPage.SiteId = defaultFrontendSite.ContentId;
                //else
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

    public static class SlugGenerator
    {
        public static string Generate(string text)
        {
            Regex regex = new Regex(@"[\s,:.;\/\\&$+@# <>\[\]{}^%]+");
            return regex.Replace(text, "-").ToLower();


            //ampersand("&")
            //dollar("$")
            //plus sign("+")
            //comma(",")
            //forward slash("/")
            //colon(":")
            //semi - colon(";")
            //equals("=")
            //question mark("?")
            //'At' symbol("@")
            //pound("#").
            //    The characters generally considered unsafe are:

            //space(" ")
            //less than and greater than("<>")
            //open and close brackets("[]")
            //open and close braces("{}")
            //pipe("|")
            //backslash("\")
            //caret("^")
            //percent("%")
        }
    }
}
