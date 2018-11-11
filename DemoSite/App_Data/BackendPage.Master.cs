using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public partial class BackendPage : System.Web.UI.MasterPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageHeader.DataBind();
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
                .FindContentVersions(By.ContentId(KnownPageIds.PageSettings),ContentEnvironment.Live)
                .Result
                .Single();

            var defaultValues = new JavaScriptSerializer().Serialize(new {SiteId = defaultSiteId});
            var newPageUri = uriBuilder.CreateUri(editPage, UriSettings.Default, new Dictionary<string, string>
            {
                ["defaultValues"]=defaultValues
            });
            Response.Redirect(newPageUri.PathAndQuery);


        }
    }
}