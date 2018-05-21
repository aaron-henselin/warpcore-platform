using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentPageId { get; set; }
        public Guid SiteId { get; set; }
        public string PageType { get; set; }
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();
        public List<PageRoute> Routes { get; set; } = new List<PageRoute>();
        public PageRoute CanonicalRoute => Routes.OrderBy(x => x.Priority).First();
        public string PhysicalFile { get; set; }
        public string RedirectExternalUrl { get; set; }
        public Guid? RedirectPageId { get; set; }
        public int SitemapPosition { get; set; }
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



        public IQueryable<CmsPage> Query(Site site = null)
        {
            using (var pagesDbContext = new PagesDbContext())
            {
                var baseQueryable =
                pagesDbContext.Pages
                    .Include(x => x.PageContent)
                    .Include(x => x.Routes);

                if (site != null)
                    return baseQueryable.AsQueryable().Where(x => x.SiteId == site.Id);
                else
                    return baseQueryable;
            }
        }

        public string CreateSlugRecursive(CmsPage cmsPage,CmsPage parentPage)
        {
            var generated = SlugGenerator.Generate(cmsPage.Name);
            if (cmsPage.ParentPageId != null)
                return parentPage.CanonicalRoute.Slug + "/" + generated;

            return "/"+generated;
        }

        private void UpdateRoutesRecursive(CmsPage cmsPage, PagesDbContext pagesDbContext)
        {
            CmsPage parentPage = null;
            if (cmsPage.ParentPageId != null)
                parentPage = pagesDbContext.Pages.Find(cmsPage.ParentPageId);

            var newDefaultSlug = CreateSlugRecursive(cmsPage, parentPage);
            if (!cmsPage.Routes.Any(x => x.Slug == newDefaultSlug))
                cmsPage.Routes.Add(new PageRoute
                {
                    Slug = newDefaultSlug
                });

            foreach (var route in cmsPage.Routes)
            {
                if (route.Slug != newDefaultSlug)
                    route.Priority = (int)RoutePriority.Former;
                else
                    route.Priority = (int)RoutePriority.Primary;
            }

            var childPages = pagesDbContext.Pages.Include(x => x.Routes).Where(x => x.Id == cmsPage.Id).ToList();
            foreach (var childPage in childPages)
                UpdateRoutesRecursive(childPage,pagesDbContext);
        }

        
        public void Save(CmsPage cmsPage)
        {
            using (var pagesDbContext = new PagesDbContext())
            {
                pagesDbContext.Pages.Update(cmsPage);
                UpdateRoutesRecursive(cmsPage,pagesDbContext);
                pagesDbContext.SaveChanges();
            }
        }
    }

    public class PagesDbContext : DbContext
    {
        public DbSet<CmsPage> Pages { get; set; }

    }

}
