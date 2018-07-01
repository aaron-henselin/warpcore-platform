using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web.Extensions;

namespace WarpCore.Web
{
    public class CmsPageBuilderContext
    {
        public CmsPage Page { get; set; }
        public ViewMode ViewMode { get; set; }

    }

    public enum ViewMode
    {
        Default,Edit
    }

    public class CmsPageBuilder
    {
        private readonly CmsPageBuilderContext _context;

        public CmsPageBuilder(CmsPageBuilderContext context)
        {
            _context = context;
        }

        public void PopulateContentPlaceholders(Page page)
        {
            foreach (var content in _context.Page.PageContent)
            {
                var ph = page.GetDescendantControls<ContentPlaceHolder>().SingleOrDefault(x => x.ID == content.ContentPlaceHolderId);
                if (ph == null)
                    ph = page.GetDescendantControls<ContentPlaceHolder>().First();

                if (_context.ViewMode == ViewMode.Edit)
                {
                    var handle = new LayoutHandle {PageContentId = content.Id};
                    ph.Controls.Add(handle);
                }

                var activatedWidget = CmsPageContentActivator.ActivateControl(content);
                ph.Controls.Add(activatedWidget);
            }
        }
    }

    public class LayoutHandle : PlaceHolder
    {
        public Guid PageContentId { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var p = new Panel();
            p.Attributes["data-wc-role"] = "layout-handle";
            p.Attributes["data-wc-page-content-id"] = PageContentId.ToString();

            this.Controls.Add(p);
        }
    }

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
                Page handlerPage = (Page)HttpContext.Current.Handler;
                handlerPage.PreInit += (sender, args) =>
                {
                    var localPage = (Page) sender;
                    var pageBuilder = Dependency.Resolve<CmsPageBuilder>();
                    pageBuilder.PopulateContentPlaceholders(localPage);
                };
            }
        }


        void IHttpModule.Dispose()
        {
        }
    }
}