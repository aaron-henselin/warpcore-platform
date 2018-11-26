using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Cms;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Platform.Orm;
using WarpCore.Web.Extensions;
using WarpCore.Web.Scripting;

namespace DemoSite
{

    [Serializable]
    public class PageTreeItem
    {
        public string Name { get; set; }
        public string VirtualPath { get; set; }
        public int Depth { get; set; }
        public string ParentPath { get; set; }
        public bool IsHomePage { get; internal set; }
        public bool IsPublished { get; set; }
        public string DesignUrl { get; set; }

        public string SettingsUrl { get; set; }
        public bool IsExpanded { get; set; }
        public bool Visible { get; set; }
        public bool HasChildItems { get; set; }
        public Guid PageId { get; set; }
    }

    [Serializable]
    public class PageTreeControlState
    {
        public List<PageTreeItem> PageTreeItems { get; set; }
        
    }

    public partial class PageTree : System.Web.UI.UserControl
    {
        PageTreeControlState _controlState = new PageTreeControlState();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);


            var selectedSite = SiteManagementContext.GetSiteToManage();
            if (!Page.IsPostBack)
            {
                RebuildControlState(selectedSite);
                DataBind();
            }

            //< asp:Button runat = "server" ID = "CreateNewPageButton" OnClick = "CreateNewPageButton_OnClick" Text = "Create new page" CssClass = "pull-right btn btn-primary" />
            var ph = Page.Master.GetDescendantControls<PlaceHolder>().First(x => x.ID == "ActionItemsPlaceHolder");
            var button = new Button();
            button.Click += CreateNewPageButton_OnClick;
            button.Text = "Create new page";
            button.CssClass = "pull-right btn btn-primary";
            ph.Controls.Add(button);
        }

        protected void CreateNewPageButton_OnClick(object sender, EventArgs e)
        {
            Guid defaultSiteId = Guid.Empty;
            var defaultFrontendSite = SiteManagementContext.GetSiteToManage();
            if (defaultFrontendSite != null)
                defaultSiteId = defaultFrontendSite.ContentId;

            var uriBuilderContext = HttpContext.Current.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            var editPage = new CmsPageRepository()
                .FindContentVersions(By.ContentId(KnownPageIds.AddPageWizard), ContentEnvironment.Live)
                .Result
                .Single();

            var defaultValues = new JavaScriptSerializer().Serialize(new { SiteId = defaultSiteId });
            var newPageUri = uriBuilder.CreateUri(editPage, UriSettings.Default, new Dictionary<string, string>
            {
                ["defaultValues"] = defaultValues
            });
            Response.Redirect(newPageUri.PathAndQuery);


        }



        protected override void LoadControlState(object savedState)
        {
            _controlState = (PageTreeControlState) savedState;
            DataBind();
        }

        protected override object SaveControlState()
        {
            return _controlState;
        }

        private void RebuildControlState(Site matchedSite)
        {
            
            var draftSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Draft,SitemapBuilderFilters.All);
            var liveSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Live, SitemapBuilderFilters.All);

            var pagesTreeItems = new List<PageTreeItem>();
            foreach (var childNode in draftSitemap.ChildNodes)
            {
                FlattenPageTree(childNode, pagesTreeItems, 0);
            }

            var uriBuilderContext = HttpContext.Current.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            

            foreach (var pageTreeItem in pagesTreeItems)
            {
                var liveNode = liveSitemap.GetSitemapNode(pageTreeItem.PageId);
                pageTreeItem.IsPublished = liveNode != null;
                pageTreeItem.IsHomePage = pageTreeItem.PageId == matchedSite.HomepageId;

                var draftNode = draftSitemap.GetSitemapNode(pageTreeItem.PageId);
                if (draftNode.Page.PageType == PageType.ContentPage)
                {
                    try
                    {
                        var uri = uriBuilder.CreateUri(draftNode.Page, UriSettings.Default, new Dictionary<string, string>
                        {
                            [PageDesignerUriComponents.ViewMode] = "PageDesigner",
                        });
                        pageTreeItem.DesignUrl = uri.ToString();
                    }
                    catch (Exception e)
                    {
                        pageTreeItem.DesignUrl = $"/Admin/draft?{PageDesignerUriComponents.ViewMode}=PageDesigner&{PageDesignerUriComponents.SiteId}={draftNode.Page.SiteId}&{PageDesignerUriComponents.PageId}={pageTreeItem.PageId}";
                    }

                }

                pageTreeItem.SettingsUrl = "/admin/settings?contentId=" + pageTreeItem.PageId;
                   
            }

            _controlState.PageTreeItems = pagesTreeItems.ToList();

            foreach (var item in _controlState.PageTreeItems)
            {
                if (item.ParentPath == null)
                {
                    item.Visible = true;
                    ExpandPath(item.VirtualPath.ToString());
                }
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            PageTreeItemRepeater.DataSource = _controlState.PageTreeItems;
            PageTreeWrapper.DataBind();
        }

        private void FlattenPageTree(SitemapNode sitemapNode, List<PageTreeItem> nodes, int depth, SitemapNode parentNode= null)
        {

            nodes.Add(new PageTreeItem {
                //SitemapNode = sitemapNode,
                Name = sitemapNode.Page.Name,
                PageId = sitemapNode.Page.ContentId,
                VirtualPath = sitemapNode.VirtualPath.ToString(),
                Depth = depth,
                ParentPath = parentNode?.VirtualPath.ToString(),
                HasChildItems = sitemapNode.ChildNodes.Any()
            });
            
            foreach (var childNode in sitemapNode.ChildNodes)
                FlattenPageTree(childNode,nodes,depth+1,sitemapNode);
          
        }

        protected void ToggleExpandPageItem_OnClick(object sender, EventArgs e)
        {
            var lb = (LinkButton) sender;
            var expandId = lb.CommandArgument;

            ExpandPath(expandId);

            this.DataBind();
        }

        private void ExpandPath(string expandId)
        {
            var item =
                _controlState.PageTreeItems.Single(x => x.VirtualPath.ToString() == expandId);

            item.IsExpanded = !item.IsExpanded;

            var childItems =
                _controlState.PageTreeItems.Where(x => x.ParentPath == expandId);

            foreach (var childItem in childItems)
                childItem.Visible = item.IsExpanded;
        }

        public override void DataBind()
        {
            base.DataBind();
      
            ControlExtensions.RegisterDescendentAsyncPostBackControl(PageTreeWrapper);
            ScriptManagerExtensions.RegisterScriptToRunEachFullOrPartialPostback(this.Page, "pagetree.init();");
        }

        protected void PublishLinkButton_OnClick(object sender, EventArgs e)
        {
            var lb = (LinkButton) sender;
            var contentId = new Guid(lb.CommandArgument);
            var pageRepository = new CmsPageRepository();
            pageRepository.Publish(By.ContentId(contentId));

            var selectedSite = SiteManagementContext.GetSiteToManage();
            RebuildControlState(selectedSite);
            DataBind();

        }
    }
}