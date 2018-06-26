using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        private void AssertSlugIsNotTaken(CmsPage cmsPage)
        {
            var condition = $@"{nameof(SiteStructureNode.PageId)} eq '{cmsPage.ContentId}'";
            var node = Orm.FindUnversionedContent<SiteStructureNode>(condition).Result.Single();

            var siblingsCondition = $@"{nameof(SiteStructureNode.PageId)} neq '{node.PageId}' and {nameof(SiteStructureNode.ParentNodeId)} eq '{node.ParentNodeId}'";
            var siblings = Orm.FindUnversionedContent<SiteStructureNode>(condition).Result.Single();




            //var dupSlugs = GetAllPages()
            //    .Where(x => x.ParentPageId == cmsPage.ParentPageId && x.Id != cmsPage.Id)
            //    .SelectMany(x => x.Routes)
            //    .Where(x => x.Priority == (int) RoutePriority.Primary);

            //if (dupSlugs.Any())
            //    throw new DuplicateSlugException();
        }

        public override void Save(CmsPage cmsPage)
        {
            if (string.IsNullOrWhiteSpace(cmsPage.Slug))
            {
                GenerateUniqueSlugForPage(cmsPage);
            }
            else
                AssertSlugIsNotTaken(cmsPage);
           

            var newVirtualPath = CreateVirtualPath(cmsPage);
            if (cmsPage.AlternateRoutes.All(x => x.VirtualPath != newVirtualPath))
                cmsPage.AlternateRoutes.Add(new PageRoute
                {
                    VirtualPath = newVirtualPath
                });

            foreach (var route in cmsPage.AlternateRoutes)
            {
                if (route.VirtualPath != newVirtualPath)
                    route.Priority = (int) RoutePriority.Former;
                else
                    route.Priority = (int) RoutePriority.Primary;
            }

            //todo: save this stuff.
            //page.Routes.Where(x => x.Slug == newDefaultSlug)

            //_dbAdapter.Save();
            //SlugGenerator.Generate(page.Name)
            //new RouteRepository().GetAllRoutes().
            //new Page().Name
            //page.Name
        }

        private void GenerateUniqueSlugForPage(CmsPage cmsPage)
        {
            bool foundUniqueSlug = false;
            cmsPage.Slug = SlugGenerator.Generate(cmsPage.Name);

            int counter = 2;
            while (!foundUniqueSlug)
            {
                var rawSlug = cmsPage.Name;
                try
                {
                    AssertSlugIsNotTaken(cmsPage);
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
