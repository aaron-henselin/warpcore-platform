using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using WarpCore.Cms.Routing;
using WarpCore.Data.Schema;

namespace WarpCore.Cms
{
    public struct PageType
    {
        public const string ContentPage="ContentPage";
        public const string GroupingPage = "GroupingPage";
        public const string RedirectPage = "RedirectPage";
    }

    [Table("cms_page")]
    public class CmsPage
    {

        [Column]
        public Guid Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public Guid? ParentPageId { get; set; }

        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public string PageType { get; set; }

        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();

        public List<PageRoute> Routes { get; set; } = new List<PageRoute>();
        public string PhysicalFile { get; set; }
        public string RedirectExternalUrl { get; set; }

        public Guid? RedirectPageId { get; set; }

        [Column]
        public int Order { get; set; }
    }

    public enum RoutePriority
    {
        Primary = 0,
        Former = 10,
    }

    [Table("cms_page_route")]
    public class PageRoute : Entity
    {
        [Column]
        public string Slug { get; set; }

        [Column]
        public int Priority { get; set; }

        [Column]
        public int Order { get; set; }
    }

    [Table("cms_page_content")]
    public class CmsPageContent
    {
        [Column]
        public string ContentPlaceHolderId { get; set; }

        [Column]
        public string WidgetTypeCode { get; set; }

        [Column]
        public Dictionary<string,string> Parameters { get; set; }
    }

    public class PageRepository
    {
        //private DbEngineAdapter _dbAdapter;

        public PageRepository()
        {
           // _dbAdapter = new DbEngineAdapter();
        }

        public CmsPage GetPage(Guid pageId)
        {
            return new CmsPage();
        }

        public IEnumerable<CmsPage> GetAllPages()
        {
            return new List<CmsPage>();
        }

        public IEnumerable<CmsPage> GetAllPages(Site site)
        {
            return new List<CmsPage>();
        }

        public string CreateSlugRecursive(CmsPage cmsPage)
        {
            var generated = SlugGenerator.Generate(cmsPage.Name);
            if (cmsPage.ParentPageId != null)
                return CreateSlugRecursive(cmsPage) + "/" + generated;

            return "/"+generated;
        }

        public void Save(CmsPage cmsPage)
        {
            var newDefaultSlug = CreateSlugRecursive(cmsPage);

            if (!cmsPage.Routes.Any(x => x.Slug == newDefaultSlug))
                cmsPage.Routes.Add(new PageRoute
                {
                    Slug = newDefaultSlug
                });

            foreach (var route in cmsPage.Routes)
            {
                if (route.Slug != newDefaultSlug)
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
    }

}
