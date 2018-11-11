using System;
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
using WarpCore.Platform.Orm;
using WarpCore.Web;

namespace DemoSite
{
    public class ToolboxItemViewModel
    {
        public string WidgetTypeCode { get; set; }
        public string FriendlyName { get; set; }
        public string Category { get; set; }
    }

    [Serializable]
    public class ToolboxControlState
    {
        public string SelectedCategory { get; set; }
    }

    public partial class Toolbox : System.Web.UI.UserControl
    {
        private ToolboxControlState _controlState = new ToolboxControlState{ SelectedCategory = "Layout"};

        private ToolboxManager _manager = new ToolboxManager();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.RegisterRequiresControlState(this);

            var allCategories = _manager.Find().Select(x => x.Category).Distinct();
            foreach (var category in allCategories)
                ToolboxCategorySelector.Items.Add(category);

            ToolboxCategorySelector.SelectedValue = _controlState.SelectedCategory;

            if (!Page.IsPostBack)
                FinishInit();
        }

        protected override void LoadControlState(object savedState)
        {
            _controlState = (ToolboxControlState)savedState;
            ToolboxCategorySelector.SelectedValue = _controlState.SelectedCategory;

            FinishInit();
        }

        private void FinishInit()
        {
            PopulateToolboxSidebar(_controlState.SelectedCategory);
        }

        protected override object SaveControlState()
        {
            return _controlState;
        }


        private void PopulateToolboxSidebar(string category)
        {
         
            ToolboxItemRepeater.DataSource = _manager.Find().Select(x => new ToolboxItemViewModel
                {
                    FriendlyName = x.FriendlyName,
                    WidgetTypeCode = x.WidgetUid,
                    Category = x.Category
                })
                .Where(x => x.Category == category)
                .ToList();
            ToolboxItemRepeater.DataBind();

        }

        protected void BackToPageTreeLinkButton_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("/admin/pages");
        }

        protected void SaveDraftButton_OnClick(object sender, EventArgs e)
        {
            SaveChangesImpl(false);
            Response.Redirect("/admin/pages");
        }

        protected void SaveAndPublishButton_OnClick(object sender, EventArgs e)
        {
            SaveChangesImpl(true);
            Response.Redirect("/admin/pages");
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
                var pageRepository = new CmsPageRepository();
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


        protected void ToolboxCategorySelector_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            _controlState.SelectedCategory = ((DropDownList) sender).SelectedValue;
            PopulateToolboxSidebar(_controlState.SelectedCategory);
        }
    }
}