using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web.Extensions;

namespace WarpCore.Web
{
    public class ControlActivator
    {
        public static Control ActivateControl(CmsPageContent pageContent)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            var toolboxItemType = Type.GetType(toolboxItem.FullyQualifiedTypeName);
            var activatedWidget = (Control)Activator.CreateInstance(toolboxItemType);
            PropertySet(activatedWidget,pageContent.Parameters);

            return activatedWidget;
        }

        public static void PropertySet(Control activatedWidget, Dictionary<string,string> parameterValues)
        {
            foreach (var kvp in parameterValues)
            {
                var propertyInfo = activatedWidget.GetType().GetProperty(kvp.Key);
                if (propertyInfo == null || !propertyInfo.CanWrite)
                    continue;

                var newType = Convert.ChangeType(kvp.Value, propertyInfo.PropertyType);
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(activatedWidget, newType);
            }
            
        }
    }

    public sealed class MyHttpModule : IHttpModule
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

            var cmsPage = new PageRepository().FindContentVersions(route.PageId.Value,ContentEnvironment.Live).Result.Single();
            if (PageType.ContentPage != cmsPage.PageType)
                return;

            var page = (Page) sender;
            foreach (var content in cmsPage.PageContent)
            {
                var ph = page.GetDescendantControls<ContentPlaceHolder>().SingleOrDefault(x => x.ID == content.ContentPlaceHolderId);
                if (ph == null)
                    ph = page.GetDescendantControls<ContentPlaceHolder>().First();


                var activatedWidget = ControlActivator.ActivateControl(content);
                ph.Controls.Add(activatedWidget);
            }
        }

        void IHttpModule.Dispose()
        {
        }
    }
}