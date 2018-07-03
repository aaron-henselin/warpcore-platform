using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Layout;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web.Extensions;
using WarpCore.Web.Scripting;
using WarpCore.Web.Widgets;

namespace WarpCore.Web
{
    public class CmsPageRequestContext
    {
        public SiteRoute Route { get; set; }
        public CmsPage CmsPage { get; set; }
        public ViewMode ViewMode { get; set; }
    }

    public enum ViewMode
    {
        Default,Edit
    }

    public class CmsPageBuilder
    {
        private readonly CmsPageRequestContext _context;

        public CmsPageBuilder(CmsPageRequestContext context)
        {
            _context = context;
        }

        public void PopulateContentPlaceholders(Page page)
        {
            if (_context.CmsPage == null)
                return;

            if (_context.ViewMode == ViewMode.Edit)
            {
                if (page.Form != null)
                {
                    page.Form.Controls.Add(new ProxiedScriptManager());
                    ScriptManagerExtensions.RegisterScriptToRunEachFullOrPartialPostback(page, "warpcore.page.edit();");
                }
            }

            foreach (var content in _context.CmsPage.PageContent)
            {
                var ph = page.Master.GetDescendantControls<ContentPlaceHolder>().SingleOrDefault(x => x.ID == content.ContentPlaceHolderId);
                if (ph == null)
                    ph = page.Master.GetDescendantControls<ContentPlaceHolder>().First();

                if (_context.ViewMode == ViewMode.Edit)
                {
                    var toolboxItem = new ToolboxManager().GetToolboxItemByCode(content.WidgetTypeCode);
                    var handle = new LayoutHandle {PageContentId = content.Id, HandleName = toolboxItem.Name};
                    ph.Controls.Add(handle);
                }

                var activatedWidget = CmsPageContentActivator.ActivateControl(content);
                ph.Controls.Add(activatedWidget);
            }
        }

        public void SetupLayout(Page localPage)
        {
            if (_context.CmsPage == null)
                return;

            var layoutToApply = new LayoutRepository().GetById(_context.CmsPage.LayoutId);
            localPage.MasterPageFile = layoutToApply.MasterPagePath;
        }
    }

    public class LayoutHandle : PlaceHolder
    {
        public Guid PageContentId { get; set; }
        public string HandleName { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var p = new Panel();
            p.Attributes["data-wc-role"] = "layout-handle";
            p.Attributes["data-wc-page-content-id"] = PageContentId.ToString();
            p.Attributes["class"] = "wc-layout-handle";
            p.Controls.Add(new Label{Text=HandleName});
            this.Controls.Add(p);
        }
    }



    public sealed class CmsPageBuilderHttpModule : IHttpModule
    {
        void IHttpModule.Init(HttpApplication application)
        {

            application.PreRequestHandlerExecute += new EventHandler(application_PreRequestHandlerExecute);
            application.BeginRequest += delegate(object sender, EventArgs args)
            {
                if (!WebBootstrapper.IsBooted)
                {
                    WebBootstrapper.EnsureSiteBootHasBeenStarted();
                    HttpContext.Current.RewritePath("/App_Data/Booting.aspx", true);

                    return;
                }

                var routingContext = HttpContext.Current.ToCmsRouteContext();
                HttpContext.Current.Request.RequestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.RouteDataToken,routingContext);

                if (routingContext.Route != null)
                {
                    var handler = new WarpCoreRequestProcessor();
                    handler.ProcessRequest(HttpContext.Current, routingContext);
                }


            };
            
        }

        static void application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
       
                RegisterPagePreRenderEventHandler();
        }

        private static void RegisterPagePreRenderEventHandler()
        {
            if (HttpContext.Current.Handler.GetType().ToString().EndsWith("_aspx"))
            { // Register PreRender handler only on aspx pages.
                Page handlerPage = (Page)HttpContext.Current.Handler;
                handlerPage.PreInit += (sender, args) =>
                {
                    
                    var rt = (CmsPageRequestContext)HttpContext.Current.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
                    if (rt == null || rt.CmsPage == null)
                        return;

                    var localPage = (Page)sender;
                    var pageBuilder = new CmsPageBuilder(rt);
                    pageBuilder.SetupLayout(localPage);
                    pageBuilder.PopulateContentPlaceholders(localPage);
                };
            }
        }


        void IHttpModule.Dispose()
        {
        }
    }
}