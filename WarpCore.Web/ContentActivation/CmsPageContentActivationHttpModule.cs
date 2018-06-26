using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web.Extensions;

namespace WarpCore.Web
{
    public sealed class CmsPageContentActivationHttpModule : IHttpModule
    {
        void IHttpModule.Init(HttpApplication application)
        {
            application.PreRequestHandlerExecute += new EventHandler(application_PreRequestHandlerExecute);
        }

        void application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            RegisterPagePreRenderEventHandler();
        }

        private void RegisterPagePreRenderEventHandler()
        {
            if (HttpContext.Current.Handler.GetType().ToString().EndsWith("_aspx"))
            { // Register PreRender handler only on aspx pages.
                Page page = (Page)HttpContext.Current.Handler;
                page.PreInit += PageOnPreInit;
            }
        }

        private void PageOnPreInit(object sender, EventArgs eventArgs)
        {
            var success = CmsRouteTable.Current.TryGetRoute(HttpContext.Current.Request.Url, out var route);
            if (!success || route.PageId == null)
                return;

            var cmsPage = new PageRepository().FindContentVersions(By.ContentId(route.PageId.Value),ContentEnvironment.Live).Result.Single();
            if (PageType.ContentPage != cmsPage.PageType)
                return;

            var page = (Page) sender;
            foreach (var content in cmsPage.PageContent)
            {
                var ph = page.GetDescendantControls<ContentPlaceHolder>().SingleOrDefault(x => x.ID == content.ContentPlaceHolderId);
                if (ph == null)
                    ph = page.GetDescendantControls<ContentPlaceHolder>().First();


                var activatedWidget = CmsPageContentActivator.ActivateControl(content);
                ph.Controls.Add(activatedWidget);
            }
        }

        void IHttpModule.Dispose()
        {
        }
    }
}