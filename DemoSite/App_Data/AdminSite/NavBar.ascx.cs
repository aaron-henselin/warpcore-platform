using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Modules.Cms.Features.Context;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public class NavBarItem
    {
        public string Url { get; set; }
        public string Text { get; set; }
        public List<NavBarItem> ChildItems { get; set; } = new List<NavBarItem>();
    }

    public partial class NavBar : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var siteRepository = new SiteRepository();
            var manageableSites = siteRepository.GetFrontendSites();

            foreach (var managedSite in manageableSites)
                SiteSelectorDropDownList.Items.Add(new ListItem(managedSite.Name, managedSite.ContentId.ToString()));

            SiteSelectorDropDownList.SelectedIndexChanged += (sender, args) =>
            {
                var isNewValueSelected = !string.IsNullOrEmpty(SiteSelectorDropDownList.SelectedValue);
                if (isNewValueSelected)
                    Response.Redirect("/admin/pagetree?siteId=" +
                                      SiteSelectorDropDownList.SelectedValue);
            };

            var rt = CmsPageRequestContext.Current;
            var adminSiteId = rt.Route.SiteId;
            var adminSite = siteRepository.GetById(adminSiteId);
            var sitemap = SitemapBuilder.BuildSitemap(adminSite,ContentEnvironment.Live,SitemapBuilderFilters.DisplayInNavigation);
            List<NavBarItem> navBarItems = new List<NavBarItem>();
            foreach (var node in sitemap.ChildNodes)
            {
                var item = CreateNavBarItem(node);
                navBarItems.Add(item);
            }
            NavBarRepeater.DataSource = navBarItems.ToList();
            NavBarRepeater.DataBind();
        }

        private NavBarItem CreateNavBarItem(SitemapNode node)
        {
            var uriBuilderContext = HttpContext.Current.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            var uri = uriBuilder.CreateUri(node.Page, UriSettings.Default,null);

            var topLevel = new NavBarItem { Text = node.Page.Name, Url = uri.ToString(), ChildItems = new List<NavBarItem>() };
            foreach (var child in node.ChildNodes)
                topLevel.ChildItems.Add(CreateNavBarItem(child));

            return topLevel;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}