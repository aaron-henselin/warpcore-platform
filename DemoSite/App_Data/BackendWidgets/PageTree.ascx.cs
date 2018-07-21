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

    public class PageTreeItem
    {
        public SitemapNode SitemapNode { get; set; }
        public int Depth { get; set; }
        public string ParentItem { get; set; }
        public bool IsHomePage { get; internal set; }
        public bool IsPublished { get; set; }
    }

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
            var draftSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Draft);
            var liveSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Live);

            var pagesTreeItems = new List<PageTreeItem>();
            foreach (var childNode in draftSitemap.ChildNodes)
            {
                AddChildNode(childNode, pagesTreeItems, 0);
            }

            foreach (var pageTreeItem in pagesTreeItems)
            {
                pageTreeItem.IsPublished = liveSitemap.GetSitemapNode(pageTreeItem.SitemapNode.Page) != null;
                pageTreeItem.IsHomePage = pageTreeItem.SitemapNode.Page.ContentId == matchedSite.HomepageId;
            }

            PageTreeItemRepeater.DataSource = pagesTreeItems;
            PageTreeWrapper.DataBind();
        }


        private void AddChildNode(SitemapNode sitemapNode, List<PageTreeItem> nodes, int depth)
        {
            var last = nodes.LastOrDefault();

            nodes.Add(new PageTreeItem {
                SitemapNode = sitemapNode,
                Depth = depth,
                ParentItem = last?.SitemapNode?.VirtualPath.ToString()
            });
            
            foreach (var childNode in sitemapNode.ChildNodes)
                AddChildNode(childNode,nodes,depth+1);
          
        }
    }
}