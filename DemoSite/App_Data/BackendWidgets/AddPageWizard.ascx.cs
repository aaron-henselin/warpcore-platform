using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms;
using Modules.Cms.Features.Context;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Web;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets;

namespace DemoSite
{
    public partial class AddPageWizard : System.Web.UI.UserControl
    {


        public const string ApiId = "wc-add-page-wizard";
        private const string PageTypeParameter = "pageType";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);


            PageTypeSelector.Items.Add(new ListItem("Content Page", KnownFormIds.ContentPageSettingsForm.ToString()));
            PageTypeSelector.Items.Add(new ListItem("Grouping Page", KnownFormIds.GroupingPageSettingsForm.ToString()));
            PageTypeSelector.Items.Add(new ListItem("Redirect Page", KnownFormIds.RedirectPageSettingsForm.ToString()));
            PageTypeSelector.TextChanged += PageTypeSelector_TextChanged;

            var pageTypeRaw = Request[PageTypeParameter];
            if (!string.IsNullOrWhiteSpace(pageTypeRaw))
                PageTypeSelector.SelectedValue = pageTypeRaw;
            else
                PageTypeSelector.SelectedValue = KnownFormIds.ContentPageSettingsForm.ToString();
            
            var pgContent = new CmsPageContent
            {
                PlacementContentPlaceHolderId = this.PageTypeSpecificSettingsPlaceHolder.PlaceHolderId,
                WidgetTypeCode = DynamicForm.ApiId,
                Parameters = new Dictionary<string, string> { ["FormId"] = PageTypeSelector.SelectedValue }
            };

            //CmsPageLayoutEngine.ActivateAndPlaceContent(this, new[] {pgContent});
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
    }
}