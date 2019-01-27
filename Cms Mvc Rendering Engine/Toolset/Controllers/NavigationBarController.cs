using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DemoSite;
using Modules.Cms.Features.Context;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web.Extensions;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc.Toolset.Controllers
{
    [WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Navigation Bar", Category = "Backend")]
    public class NavigationBarController : Controller
    {
        
        private SiteRepository _siteRepository = new SiteRepository();
        private const string ApiId = "wc-navigation-bar";

        private IReadOnlyCollection<NavBarItem> CreateNavBarItems()
        {
            var rt = CmsPageRequestContext.Current;
            var siteId = rt.Route.SiteId;
            var adminSite = _siteRepository.GetById(siteId);
            var sitemap = SitemapBuilder.BuildSitemap(adminSite, ContentEnvironment.Live, SitemapBuilderFilters.DisplayInNavigation);
            List<NavBarItem> navBarItems = new List<NavBarItem>();
            foreach (var node in sitemap.ChildNodes)
            {
                var item = CreateNavBarItem(node);
                navBarItems.Add(item);
            }
            return navBarItems;
        }

        private NavBarItem CreateNavBarItem(SitemapNode node)
        {
            //var uriBuilderContext = System.Web.HttpContext.Current.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder();
            var uri = uriBuilder.CreateUri(node.Page, UriSettings.Default, null);

            var topLevel = new NavBarItem { Text = node.Page.Name, Url = uri.ToString(), ChildItems = new List<NavBarItem>() };
            foreach (var child in node.ChildNodes)
                topLevel.ChildItems.Add(CreateNavBarItem(child));

            return topLevel;
        }

        public ViewResult Index()
        {
            var frontendSites = _siteRepository.GetFrontendSites();

            var rt = CmsPageRequestContext.Current;
            var siteId = rt.Route.SiteId;
            var site =_siteRepository.GetById(siteId);
            var livePage = new CmsPageRepository()
                .FindContentVersions(By.ContentId(site.HomepageId.Value), ContentEnvironment.Live).Result;

            var uriBuilder = new CmsUriBuilder();
            var uri = uriBuilder.CreateUri(
                livePage.Single(), 
                UriSettings.Default,
                new Dictionary<string, string>{["SiteId"]=siteId.ToString()});


            return View(new NavigationBarViewModel
            {
                Sites =frontendSites.Select(x => new NavBarItem{Text=x.Name,Url = uri.ToString()}).ToList(),
                TopLevelNavigationItems = CreateNavBarItems().ToList()
            });
        }

    }

    public class NavigationBarViewModel
    {
        public IReadOnlyCollection<NavBarItem> TopLevelNavigationItems { get; set; }
        public IReadOnlyCollection<NavBarItem> Sites { get; set; }
    }

    public class NavBarItem
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public List<NavBarItem> ChildItems { get; set; } = new List<NavBarItem>();
    }

    

}
