using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Web;

namespace DemoSite
{
    public partial class AddPageWizard : System.Web.UI.UserControl, IHasInternalLayout
    {


        public const string ApiId = "wc-add-page-wizard";
        private const string PageTypeParameter = "pageType";

        public Guid? GetSelectedFormFromQuerystring()
        {
            var pageTypeRaw = HttpContext.Current.Request[PageTypeParameter];
            if (string.IsNullOrWhiteSpace(pageTypeRaw))
                return null;

            return
            new[]{KnownFormIds.ContentPageSettingsForm,
                KnownFormIds.GroupingPageSettingsForm,
                KnownFormIds.RedirectPageSettingsForm}.Single(x => new Guid(pageTypeRaw) == x);
        }

        protected override void OnInit(EventArgs e)
        {
            EnsureInitialized();

            base.OnInit(e);
            PageTypeSelector.Items.Add(new ListItem("Content Page", KnownFormIds.ContentPageSettingsForm.ToString()));
            PageTypeSelector.Items.Add(new ListItem("Grouping Page", KnownFormIds.GroupingPageSettingsForm.ToString()));
            PageTypeSelector.Items.Add(new ListItem("Redirect Page", KnownFormIds.RedirectPageSettingsForm.ToString()));
            PageTypeSelector.TextChanged += PageTypeSelector_TextChanged;
            PageTypeSelector.SelectedValue = GetSelectedFormFromQuerystring()?.ToString();
        }

        private void PageTypeSelector_TextChanged(object sender, EventArgs e)
        {
            var parameters = Request.QueryString.ToDictionary();
            parameters[PageTypeParameter] = PageTypeSelector.SelectedValue;

            
            var newUrl = new CmsUriBuilder().CreateUri(CmsPageRequestContext.Current.CmsPage, UriSettings.Default,parameters);

            Response.Redirect(newUrl.ToString());
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private void EnsureInitialized()
        {
            if (this.FindControl("PageTypeSpecificSettingsPlaceHolder") == null)
            {
                PageTypeSpecificSettingsPlaceHolder = new PlaceHolder {ID = "PageTypeSpecificSettingsPlaceHolder"};
                this.Controls.Add(PageTypeSpecificSettingsPlaceHolder);
            }
        }

        public PlaceHolder PageTypeSpecificSettingsPlaceHolder { get; set; }

        public InternalLayout GetInternalLayout()
        {
            EnsureInitialized();

            var layout = new InternalLayout();
            layout.PlaceHolderIds.Add(nameof(PageTypeSpecificSettingsPlaceHolder));

            var selected = GetSelectedFormFromQuerystring();
            if (selected == null)
                return layout;

            var pgContent = new PageContent
            {
                Id = new Guid("b8029034-01db-47fe-8db8-3a1700d9caf1"), //doesn't matter what this is, but it needs to be consistent (i think?)
                PlacementContentPlaceHolderId = nameof(PageTypeSpecificSettingsPlaceHolder),
                WidgetTypeCode = DynamicForm.ApiId,
                Parameters = new Dictionary<string, string> { ["FormId"] = selected.ToString() }
            };

            layout.DefaultContent.Add(pgContent);
            return layout;
        }
    }
}