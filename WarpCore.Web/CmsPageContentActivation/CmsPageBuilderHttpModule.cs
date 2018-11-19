using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using WarpCore.Cms;
using WarpCore.Web.Extensions;

namespace WarpCore.Web
{



public interface ILayoutHandle
    {
        string FriendlyName { get; set; }
        string HandleName { get; set; }
         Guid PageContentId { get; set; }
    }

    public class CmsPageRequestContext
    {
        public SiteRoute Route { get; set; }
        public CmsPage CmsPage { get; set; }
        public PageRenderMode PageRenderMode { get; set; }

        public static CmsPageRequestContext Current=>(CmsPageRequestContext) HttpContext.Current.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
    
    }

    public enum PageRenderMode
    {
        Readonly,PageDesigner
    }

    public enum DropTargetDirective { Begin,End}

    
    public class EditingContext : IPageContent
    {
        public List<CmsPageContent> AllContent { get; set; }

        public bool IsEditing => AllContent != null;
        public Guid DesignedContentId { get; set; }
        public string DesignType { get; set; }
        public Guid DesignContentTypeId { get; set; }
    }

    public struct EditingContextVars
    {
        public const string SerializedPageDesignStateKey = "WC_EDITING_CONTEXT_JSON";
        public const string PageDesignContextKey = "WC_EDITING_CONTEXT";

        public const string ClientSideToolboxStateKey = "WC_TOOLBOX_STATE";
        public const string ClientSideConfiguratorStateKey = "WC_CONFIGURATOR_STATE";

        public const string EditingContextSubmitKey = "WC_EDITING_SUBMIT";
    }

    public class EditingContextManager
    {
        private JavaScriptSerializer _js;

        public EditingContextManager()
        {
            _js = new JavaScriptSerializer();
        }




        private EditingContext CreateEditingContext(IHasDesignedLayout hasDesignedLayout)
        {
            var ec = new EditingContext
            {
                DesignType = hasDesignedLayout.GetType().AssemblyQualifiedName,
                DesignedContentId = hasDesignedLayout.DesignForContentId,
                DesignContentTypeId = hasDesignedLayout.ContentTypeId,
                AllContent = hasDesignedLayout.DesignedContent,
            };
            var raw = _js.Serialize(ec);
            return _js.Deserialize<EditingContext>(raw);
        }

        public EditingContext GetOrCreateEditingContext(IHasDesignedLayout hasDesignedLayout)
        {

            var pageDesignHasNotStarted =
                HttpContext.Current.Request[EditingContextVars.SerializedPageDesignStateKey] == null;

            if (pageDesignHasNotStarted)
                HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] = CreateEditingContext(hasDesignedLayout);

            return GetEditingContext();
        }

        public EditingContext GetEditingContext()
        {
            if (HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] == null)
            {
                var json = HttpContext.Current.Request[EditingContextVars.SerializedPageDesignStateKey];
                HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] = _js.Deserialize<EditingContext>(json);
            }

            return (EditingContext)HttpContext.Current.Items[EditingContextVars.PageDesignContextKey];
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
       
                WireUpPreInit();
        }

        private static void WireUpPreInit()
        {
            if (HttpContext.Current.Handler.GetType().ToString().EndsWith("_aspx"))
            { // Register PreRender handler only on aspx pages.
                Page handlerPage = (Page)HttpContext.Current.Handler;
                handlerPage.PreInit += (sender, args) =>
                {
                    var rt = CmsPageRequestContext.Current;
                    var isUnmanagedAspxPage = rt == null || rt.CmsPage == null;
                    if (isUnmanagedAspxPage)
                        return;

                    var localPage = (Page)sender;
                    localPage.EnableViewState = rt.CmsPage.EnableViewState;
                    localPage.Title = rt.CmsPage.Name;
                    localPage.MetaKeywords = rt.CmsPage.Keywords;
                    localPage.MetaDescription = rt.CmsPage.Description;
                    var pageBuilder = new CmsPageLayoutEngine(rt);
                    pageBuilder.ActivateAndPlaceLayoutContent(localPage);

                    var allContent = rt.CmsPage.PageContent;
                    if (rt.PageRenderMode == PageRenderMode.PageDesigner)
                    {
                        var editing = new EditingContextManager();
                        var context = editing.GetOrCreateEditingContext(rt.CmsPage);
                        allContent = context.AllContent;
                    }

                    pageBuilder.ActivateAndPlaceAdHocPageContent(localPage, allContent);

                    if (rt.PageRenderMode == PageRenderMode.PageDesigner)
                        localPage.Init += (x, y) => localPage.EnableDesignerDependencies();

                };
                handlerPage.Init += (sender, args) =>
                {
                    handlerPage.Form.Action = HttpContext.Current.Request.RawUrl;
                };
            }
        }


        void IHttpModule.Dispose()
        {
        }
    }
}