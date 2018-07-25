﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Cms.Forms;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;

namespace DemoSite
{
    public class ToolboxItemViewModel
    {
        public string WidgetTypeCode { get; set; }

        public string FriendlyName { get; set; }
    }

    public partial class Toolbox : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            PopulateToolboxSidebar();

            DataBind();
        }

        private void PopulateToolboxSidebar()
        {
            var manager = new ToolboxManager();
            var allWidgets = manager.Find();
            ToolboxItemRepeater.DataSource = allWidgets.Select(x => new ToolboxItemViewModel
            {
                FriendlyName = x.FriendlyName,
                WidgetTypeCode = x.WidgetUid
            }).ToList();
            var allCategories = allWidgets.Select(x => x.Category).Distinct();
            foreach (var category in allCategories)
                ToolboxCategorySelector.Items.Add(category);

            //foreach (var widget in allWidgets)
            //{
            //    var div = new HtmlGenericControl("div");
            //    div.Attributes["class"] = "toolbox-item wc-layout-handle";
            //    div.Attributes["data-wc-toolbox-item-name"] = widget.Name;

            //    div.InnerText = widget.Name;
            //    toolboxUl.Controls.Add(div);
            //}

            //toolboxUl.Controls.Add(div);
        }

        protected void BackToPageTreeLinkButton_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("/admin/pagetree");
        }

        protected void SaveDraftButton_OnClick(object sender, EventArgs e)
        {
            SaveChangesImpl(false);
            Response.Redirect("/admin/pagetree");
        }

        protected void SaveAndPublishButton_OnClick(object sender, EventArgs e)
        {
            SaveChangesImpl(true);
            Response.Redirect("/admin/pagetree");
        }

        private static void SaveChangesImpl(bool publish)
        {
            var mgr = new EditingContextManager();
            var editingContext = mgr.GetEditingContext();

            var editingTypes = new[]
            {
                typeof(CmsPage).AssemblyQualifiedName,
                typeof(CmsForm).AssemblyQualifiedName
            };

            var editingTypeString = editingTypes.Single(x => editingContext.DesignForContentType == x);
            var editingType = Type.GetType(editingTypeString);

            
            if (editingType == typeof(CmsPage))
            {
                var pageRepository = new PageRepository();
                var pageToUpdate = pageRepository.FindContentVersions(By.ContentId(editingContext.DesignForContentId), ContentEnvironment.Draft)
                    .Result.Single();
                pageToUpdate.PageContent = editingContext.AllContent;

                pageRepository.Save(pageToUpdate);
                if (publish)
                    pageRepository.Publish(By.ContentId(editingContext.DesignForContentId));
            }

            if (editingType == typeof(CmsForm))
            {
                var formRepository = new FormRepository();
                var pageToUpdate = formRepository.FindContentVersions(By.ContentId(editingContext.DesignForContentId), ContentEnvironment.Draft)
                    .Result.SingleOrDefault();
                if (pageToUpdate == null)
                    pageToUpdate = new CmsForm();

                pageToUpdate.FormContent = editingContext.AllContent;

                formRepository.Save(pageToUpdate);
                if (publish)
                    formRepository.Publish(By.ContentId(editingContext.DesignForContentId));
            }

        }

        
    }
}