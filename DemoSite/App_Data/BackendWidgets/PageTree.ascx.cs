using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;

namespace DemoSite
{
    public partial class PageTree : System.Web.UI.UserControl
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SiteSelectorDropDownList.SelectedIndexChanged += (sender, args) =>
            {
                var isNewValueSelected = !string.IsNullOrEmpty(SiteSelectorDropDownList.SelectedValue);
                if (isNewValueSelected)
                    HttpContext.Current.Response.Redirect("?siteId=" +
                                                          SiteSelectorDropDownList.SelectedValue);

            };


            var siteRepository = new SiteRepository();
            var allSites = siteRepository.Find();
            foreach (var site in allSites)
                SiteSelectorDropDownList.Items.Add(new ListItem(site.Name,site.ContentId.Value.ToString()));

            Guid siteId = Guid.Empty;
            var siteToManageRaw = Request["siteId"];
            if (!string.IsNullOrWhiteSpace(siteToManageRaw))
                siteId = new Guid(siteToManageRaw);
            else if (allSites.Any())
                siteId = allSites.First().ContentId.Value;

            if (siteId == Guid.Empty)
                return;

            var matchedSite = allSites.Single(x => x.ContentId == siteId);
            var fullSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Draft);
            var ul = BuildPageList(fullSitemap);
            PageTreeWrapper.Controls.Add(ul);
        }

        private HtmlGenericControl BuildPageList(ISitemapNode sitemapNode)
        {
            var ul = new HtmlGenericControl("ul");

            foreach (var childNode in sitemapNode.ChildNodes)
            {
                var li = new HtmlGenericControl("li")
                {
                    InnerText = childNode.Page.Name
                };
                var subPageList = BuildPageList(childNode);
                li.Controls.Add(subPageList);
                ul.Controls.Add(li);
            }

            return ul;
        }
    }
}