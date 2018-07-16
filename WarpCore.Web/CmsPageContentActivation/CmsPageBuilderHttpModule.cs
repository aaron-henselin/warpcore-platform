using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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
    public interface ILayoutHandle
    {
         string HandleName { get; set; }
         Guid PageContentId { get; set; }
    }

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

    public enum DropTargetDirective { Begin,End}

    
    public class EditingContext : IPageContent
    {
        public List<CmsPageContent> SubContent { get; set; }

        public bool IsEditing => SubContent != null;
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

        public void EnableEditingCommands(Page page)
        {
            page.PreRender += (sender, args) =>
            {

            };

        }

        private EditingContext CreateEditingContext(CmsPage cmsPage)
        {
            var raw = _js.Serialize(new EditingContext { SubContent = cmsPage.PageContent });
            return _js.Deserialize<EditingContext>(raw);
        }

        public EditingContext GetOrCreateEditingContext(CmsPage cmsPage)
        {
            var pageDesignHasNotStarted =
                HttpContext.Current.Request[EditingContextVars.SerializedPageDesignStateKey] == null;

            if (pageDesignHasNotStarted)
                HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] = CreateEditingContext(cmsPage);

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

    public class CmsPageBuilder
    {
        private readonly CmsPageRequestContext _context;
        private readonly LayoutRepository layoutRepository = new LayoutRepository();
        public CmsPageBuilder(CmsPageRequestContext context)
        {
            _context = context;
        }

        private class LayoutPlacement
        {
            public LayoutControl ActivatedControl { get; set; }

        }


        public IReadOnlyCollection<Control> ActivateAndPlaceContent(Page page, IReadOnlyCollection<CmsPageContent> contents, ViewMode vm)
        {
            List<Control> activatedControls = new List<Control>();

            foreach (var content in contents)
            {
                var activatedWidget = CmsPageContentActivator.ActivateControl(content);

                var layoutWidget = activatedWidget as LayoutControl;
                layoutWidget?.InitializeLayout();

                var placementPlaceHolder = FindPlacementLocation(page, content);
                if (placementPlaceHolder == null)
                    continue;

                //if (vm == ViewMode.Edit)
                //    placementPlaceHolder.Controls.Add(new Literal { Text = $"<wc-page-content data-wc-page-content-id='{content.Id}'>" });

                if (vm == ViewMode.Edit)
                    AddLayoutHandle(placementPlaceHolder, content);

                if (vm == ViewMode.Edit)
                    placementPlaceHolder.Controls.Add(new Literal { Text = $"<wc-widget-render data-wc-layout='{layoutWidget != null}' data-wc-page-content-id='{content.Id}'>" });

                placementPlaceHolder.Controls.Add(activatedWidget);

                if (vm == ViewMode.Edit)
                    placementPlaceHolder.Controls.Add(new Literal { Text = "</wc-widget-render>" });

                //if (vm == ViewMode.Edit)
                //    placementPlaceHolder.Controls.Add(new Literal { Text = $"</wc-page-content>" });


                activatedControls.Add(activatedWidget);

                var newlyGeneratedPlaceholders = layoutWidget?.GetDescendantControls<ContentPlaceHolder>();
                if (content.SubContent.Any())
                {
                    var subCollection = ActivateAndPlaceContent(page, content.SubContent, vm);
                    activatedControls.AddRange(subCollection);
                }

                if (vm == ViewMode.Edit)
                {
                    if (layoutWidget != null)
                    {

                        foreach (var leaf in newlyGeneratedPlaceholders)
                        {
                            leaf.Controls.AddAt(0, new DropTarget(leaf, DropTargetDirective.Begin));
                            leaf.Controls.Add(new DropTarget(leaf, DropTargetDirective.End));
                        }
                    }
                }

            }

            return activatedControls;
        }

        private static void AddLayoutHandle(ContentPlaceHolder ph, CmsPageContent content)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(content.WidgetTypeCode);
            var ascx = BuildManager.GetCompiledType("/App_Data/PageDesignerComponents/LayoutHandle.ascx");
            var uc = (ILayoutHandle)Activator.CreateInstance(ascx);
            uc.HandleName = toolboxItem.Name;
            uc.PageContentId = content.Id;

            ph.Controls.Add((Control)uc);
        }

        private static ContentPlaceHolder FindPlacementLocation(Page page, CmsPageContent content)
        {
            Control searchContext = page.Master;
            if (content.PlacementLayoutBuilderId != null)
            {
                var layoutControls = searchContext.GetDescendantControls<LayoutControl>().ToList();
                var subLayout =
                    layoutControls.SingleOrDefault(x => x.LayoutBuilderId == content.PlacementLayoutBuilderId);

                if (subLayout != null)
                    searchContext = subLayout;
            }

            var ph = searchContext.GetDescendantControls<ContentPlaceHolder>()
                .FirstOrDefault(x => x.ID == content.PlacementContentPlaceHolderId); //need first because of nested layouts. this can go back to single when I'm not sleepy.
            if (ph == null)
                ph = searchContext.GetDescendantControls<ContentPlaceHolder>().FirstOrDefault();
            if (ph == null)
                ph = page.Master.GetDescendantControls<ContentPlaceHolder>().FirstOrDefault();
            return ph;
        }

        private class DropTarget : PlaceHolder
        {
            private readonly string _directive;


            public DropTarget()
            {
            }

            public DropTarget(ContentPlaceHolder leaf, DropTargetDirective directive)
            {
                _directive = directive.ToString();
                PlaceHolderId = leaf.ID;
                LayoutBuilderId = (leaf as LayoutBuilderContentPlaceHolder)?.LayoutBuilderId;
            }

            protected override void Render(HtmlTextWriter writer)
            {
                if (_directive == DropTargetDirective.Begin.ToString())
                    writer.Write($"<wc-droptarget data-wc-placeholder-id='{PlaceHolderId}' data-wc-layout-builder-id='{LayoutBuilderId}' data-wc-before-page-content-id='{BeforePageContentId}'>");

                if (_directive == DropTargetDirective.End.ToString())
                    writer.Write("</wc-droptarget>");               
            }

            public Guid? LayoutBuilderId { get; set; }

            public string PlaceHolderId { get; set; }

            public Guid? BeforePageContentId { get; set; }
        }

        public IReadOnlyCollection<ContentPlaceHolder> IdentifyLeaves(Page page)
        {
            List<ContentPlaceHolder> phs = new List<ContentPlaceHolder>();
            var allPhs = page.Master.GetDescendantControls<ContentPlaceHolder>();

            foreach (var ph in allPhs)
            {
                var isLeaf = !ph.GetDescendantControls<ContentPlaceHolder>().Any();
                if (isLeaf)
                {
                   phs.Add(ph);
                }
            }

            return phs;
        }
        

        public void ActivateAndPlaceAdHocPageContent(Page page)
        {
            if (_context.CmsPage == null)
                return;

            var allContent = _context.CmsPage.PageContent;

            if (_context.ViewMode == ViewMode.Edit)
            {
                var editing = new EditingContextManager();
                var context = editing.GetOrCreateEditingContext(_context.CmsPage);
                editing.EnableEditingCommands(page);
                //editing.ProcessEditingCommands(context);
                allContent = context.SubContent;
            }

            var leaves = IdentifyLeaves(page);

            ActivateAndPlaceContent(page, allContent, _context.ViewMode);

            foreach (var leaf in leaves)
                leaf.Controls.AddAt(0,new DropTarget(leaf,DropTargetDirective.Begin));

            //ActivateAndPlaceContent(page,_context.CmsPage.PageContent,_context.ViewMode);
            
            foreach (var leaf in leaves)
                leaf.Controls.Add(new DropTarget(leaf, DropTargetDirective.End));

            if (_context.ViewMode == ViewMode.Edit)
            {
                page.PreRender += (sender, args) =>
                {
                    page.Form.Controls.Add(new ProxiedScriptManager());
                    ScriptManagerExtensions.RegisterScriptToRunEachFullOrPartialPostback(page, "warpcore.page.edit();");



                };
            }
        }



        public void ActivateAndPlaceInheritedContent(Page localPage)
        {
            var layoutToApply = layoutRepository.GetById(_context.CmsPage.LayoutId);          
            var structure=layoutRepository.GetLayoutStructure(layoutToApply);
            var lns = FlattenLayoutTree(structure);

            if (lns.Any())
            {
                localPage.MasterPageFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
            }
            else
                localPage.MasterPageFile = "/App_Data/AdHocLayoutMaster.master";

            foreach (var ln in lns)
                ActivateAndPlaceContent(localPage,ln.Layout.PageContent,ViewMode.Default);
            
        }

        private static IReadOnlyCollection<LayoutNode> FlattenLayoutTree(LayoutNode ln)
        {
            int depth = 0;
            List<LayoutNode> lns = new List<LayoutNode>();

            var currentNode = ln;
            while (currentNode != null)
            {
                if (depth > 255)
                    throw new Exception("Recursive layout.");

                lns.Add(currentNode);
                currentNode = ln.ParentNode;
                depth++;
            }

            lns.Reverse();
            return lns;
        }


    }

//    public class LayoutHandleOld : PlaceHolder
//    {
//        public Guid PageContentId { get; set; }
//        public string HandleName { get; set; }

//        protected override void OnInit(EventArgs e)
//        {
//            base.OnInit(e);

//            var literal = new Literal();
//            literal.Text = $@"
//<li class=""StackedListItem StackedListItem--isDraggable wc-layout-handle"" tabindex=""1""  data-wc-page-content-id=""{PageContentId}"" >
//<div class=""StackedListContent"">
//<h4 class=""Heading Heading--size4 text-no-select"">{HandleName}

//<div class='pull-right wc-edit-command configure' data-wc-widget-type=""{HandleName}"" data-wc-editing-command-configure=""{PageContentId}"" >settings</div>
//<div class='pull-right wc-edit-command delete' data-wc-editing-command-delete=""{PageContentId}"" >X</div>
//</h4>
//<div class=""DragHandle""></div>
//<div class=""Pattern Pattern--typeHalftone""></div>
//<div class=""Pattern Pattern--typePlaced""></div></div></li>
//            ";

//            //var p = new Panel();
//            //p.Attributes["data-wc-role"] = "layout-handle";
//            //p.Attributes["data-wc-page-content-id"] = PageContentId.ToString();
//            //p.Attributes["class"] = "wc-layout-handle";
//            //p.Controls.Add(new Label{Text=HandleName});
//            this.Controls.Add(literal);
//        }
//    }



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
                //HttpContext.Current.Request.RequestContext.RouteData.DataTokens.Add(CmsRouteDataTokens.OriginalUriToken, HttpContext.Current.Request.Url.AbsolutePath);

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
                    var rt = (CmsPageRequestContext)HttpContext.Current.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
                    var isUnmanagedAspxPage = rt == null || rt.CmsPage == null;
                    if (isUnmanagedAspxPage)
                        return;

                    var localPage = (Page)sender;
                    var pageBuilder = new CmsPageBuilder(rt);
                    pageBuilder.ActivateAndPlaceInheritedContent(localPage);
                    pageBuilder.ActivateAndPlaceAdHocPageContent(localPage);

                    if (rt.ViewMode == ViewMode.Edit)
                    {
                        var htmlForm = localPage.GetDescendantControls<HtmlForm>().Single();
                        var bundle = new AscxPlaceHolder{VirtualPath = "/App_Data/PageDesignerComponents/PageDesignerControlSet.ascx"};
                        htmlForm.Controls.Add(bundle);
                    }
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