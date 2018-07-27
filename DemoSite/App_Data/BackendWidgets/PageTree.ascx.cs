﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Cms;
using WarpCore.Cms;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;
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
            
            var draftSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Draft);
            var liveSitemap = SitemapBuilder.BuildSitemap(matchedSite, ContentEnvironment.Live);

            var pagesTreeItems = new List<PageTreeItem>();
            foreach (var childNode in draftSitemap.ChildNodes)
            {
                FlattenPageTree(childNode, pagesTreeItems, 0);
            }

            foreach (var pageTreeItem in pagesTreeItems)
            {
                var liveNode = liveSitemap.GetSitemapNode(pageTreeItem.PageId);
                pageTreeItem.IsPublished = liveNode != null;
                pageTreeItem.IsHomePage = pageTreeItem.PageId == matchedSite.HomepageId;
                pageTreeItem.DesignUrl = pageTreeItem.VirtualPath + "?wc-viewmode=PageDesigner&wc-pg=" +
                                         pageTreeItem.PageId;
               
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
            var pageRepository = new PageRepository();
            pageRepository.Publish(By.ContentId(contentId));

            var selectedSite = SiteManagementContext.GetSiteToManage();
            RebuildControlState(selectedSite);
            DataBind();

        }
    }
}