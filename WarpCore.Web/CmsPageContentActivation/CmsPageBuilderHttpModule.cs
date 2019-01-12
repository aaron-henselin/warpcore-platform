using System;
using System.Web;
using Modules.Cms.Features.Context;
using WarpCore.Cms;
using WarpCore.Web.Extensions;

namespace WarpCore.Web
{
    
    public sealed class CmsPageBuilderHttpModule : IHttpModule
    {
        void IHttpModule.Init(HttpApplication application)
        {
            application.PostMapRequestHandler += (sender, args) =>
            {
                if (CmsPageRequestContext.Current?.CmsPage != null)
                    HttpContext.Current.Handler = new ContentPageHandler();

            };
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

                //todo: handle 404 by system?

            };
            
        }
        public struct CmsRouteDataTokens
        {
            public const string RouteDataToken = "WC_RTCONTEXT";
        }

        public static CmsPageRequestContext RequestContext =>
            (CmsPageRequestContext)HttpContext.Current.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];

        static void application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
       
                WireUpPreInit();
        }

        private static void WireUpPreInit()
        {
            //if (HttpContext.Current.Handler.GetType().ToString().EndsWith("_aspx"))
            //{ // Register PreRender handler only on aspx pages.
            //    Page handlerPage = (Page)HttpContext.Current.Handler;
            //    handlerPage.PreInit += (sender, args) =>
            //    {
            //        var rt = CmsPageRequestContext.Current;
            //        var isUnmanagedAspxPage = rt == null || rt.CmsPage == null;
            //        if (isUnmanagedAspxPage)
            //            return;
                    

            //        var localPage = (Page)sender;
            //        localPage.EnableViewState = rt.CmsPage.EnableViewState;
            //        localPage.Title = rt.CmsPage.Name;
            //        localPage.MetaKeywords = rt.CmsPage.Keywords;
            //        localPage.MetaDescription = rt.CmsPage.Description;
            //        var pageBuilder = new CmsPageLayoutEngine(rt);

            //        var page = new PageRendering();

            //        pageBuilder.AddLayoutContent(page);

            //        var allContent = rt.CmsPage.PageContent;
            //        if (rt.PageRenderMode == PageRenderMode.PageDesigner)
            //        {
            //            var editing = new EditingContextManager();
            //            var context = editing.GetOrCreateEditingContext(rt.CmsPage);
            //            allContent = context.AllContent;
            //        }

            //        var d = page.GetPartialPageRenderingByLayoutBuilderId();
            //        foreach (var contentItem in allContent)
            //        {
            //            var placementLayoutBuilderId = contentItem.PlacementLayoutBuilderId ?? Guid.Empty;
            //            var root = d[placementLayoutBuilderId];
            //            pageBuilder.AddAdHocContent(root, contentItem );
            //        }

            //        if (rt.PageRenderMode == PageRenderMode.PageDesigner)
            //            localPage.Init += (x, y) => localPage.EnableDesignerDependencies();

            //    };
            //    handlerPage.Init += (sender, args) =>
            //    {
            //        handlerPage.Form.Action = HttpContext.Current.Request.RawUrl;
            //    };
            //}
        }


        void IHttpModule.Dispose()
        {
        }
    }
}