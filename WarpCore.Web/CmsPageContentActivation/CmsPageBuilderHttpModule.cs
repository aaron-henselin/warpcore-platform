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
using Cms;
using Cms.Forms;
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
        public PageRenderMode PageRenderMode { get; set; }
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
        public Guid DesignForContentId { get; set; }
        public string DesignForContentType { get; set; }
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
                DesignForContentType = hasDesignedLayout.GetType().AssemblyQualifiedName,
            DesignForContentId = hasDesignedLayout.DesignForContentId,
                AllContent = hasDesignedLayout.DesignedContent
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



    public class CmsPageBuilder
    {
        private readonly CmsPageRequestContext _context;
        private readonly LayoutRepository layoutRepository = new LayoutRepository();
        public CmsPageBuilder(CmsPageRequestContext context)
        {
            _context = context;
        }



        public static IReadOnlyCollection<Control> ActivateAndPlaceContent(Control placementSearchContext, IReadOnlyCollection<CmsPageContent> contents, PageRenderMode vm = PageRenderMode.Readonly)
        {
            List<Control> activatedControls = new List<Control>();

            foreach (var content in contents)
            {
                var activatedWidget = CmsPageContentActivator.ActivateControl(content);

                var layoutWidget = activatedWidget as LayoutControl;
                if (layoutWidget != null)
                {
                    layoutWidget.LayoutBuilderId = content.Id;
                    layoutWidget.InitializeLayout();
                }


                var placementPlaceHolder = FindPlacementLocation(placementSearchContext, content);
                if (placementPlaceHolder == null)
                    continue;

                //if (vm == ViewMode.Edit)
                //    placementPlaceHolder.Controls.Add(new Literal { Text = $"<wc-page-content data-wc-page-content-id='{content.Id}'>" });

                if (vm == PageRenderMode.PageDesigner)
                    AddLayoutHandle(placementPlaceHolder, content);


                if (vm == PageRenderMode.PageDesigner)
                    placementPlaceHolder.Controls.Add(new Literal
                    {
                        Text =
                            $"<wc-widget-render data-wc-layout='{layoutWidget != null}' data-wc-page-content-id='{content.Id}'>"
                    });

                placementPlaceHolder.Controls.Add(activatedWidget);

                if (vm == PageRenderMode.PageDesigner)
                    placementPlaceHolder.Controls.Add(new Literal {Text = "</wc-widget-render>"});


                //if (vm == ViewMode.Edit)
                //    placementPlaceHolder.Controls.Add(new Literal { Text = $"</wc-page-content>" });


                activatedControls.Add(activatedWidget);

                var newlyGeneratedPlaceholders = layoutWidget?.GetDescendantControls<ContentPlaceHolder>();
                if (content.AllContent.Any())
                {
                    var subCollection = ActivateAndPlaceContent(activatedWidget, content.AllContent, vm);
                    activatedControls.AddRange(subCollection);
                }

                if (vm == PageRenderMode.PageDesigner)
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

        private static void AddLayoutHandle(Control ph, CmsPageContent content)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(content.WidgetTypeCode);
            var ascx = BuildManager.GetCompiledType("/App_Data/PageDesignerComponents/LayoutHandle.ascx");
            var uc = (ILayoutHandle)Activator.CreateInstance(ascx);
            uc.HandleName = toolboxItem.WidgetUid;
            uc.PageContentId = content.Id;

            ph.Controls.Add((Control)uc);
        }

        private static Control FindPlacementLocation(Control searchContext,CmsPageContent content)
        {

            if (content.PlacementLayoutBuilderId != null)
            {
                var subLayout =
                    searchContext.FindDescendantControlOrSelf<LayoutControl>(x =>
                        x.LayoutBuilderId == content.PlacementLayoutBuilderId);

                //searchContext.GetDescendantControls<LayoutControl>().ToList();
                //var subLayout =
                //    layoutControls.SingleOrDefault(x => x.LayoutBuilderId == content.PlacementLayoutBuilderId);

                if (subLayout != null)
                    searchContext = subLayout;
            }

            Control ph;

            ph = searchContext.FindDescendantControlOrSelf<ContentPlaceHolder>(x =>
                x.ID == content.PlacementContentPlaceHolderId);

            if (ph == null)
                ph = searchContext.FindDescendantControlOrSelf<RuntimeContentPlaceHolder>(x =>
                    x.PlaceHolderId == content.PlacementContentPlaceHolderId);

            if (ph == null)
                ph = searchContext.FindDescendantControlOrSelf<ContentPlaceHolder>(x => true);

            return ph;
        }

        public class DropTarget : PlaceHolder
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

            public DropTarget(RuntimeContentPlaceHolder leaf, DropTargetDirective directive)
            {
                _directive = directive.ToString();
                PlaceHolderId = leaf.PlaceHolderId;
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

        public IReadOnlyCollection<ContentPlaceHolder> IdentifyLayoutLeaves(Control searchRoot)
        {
            List<ContentPlaceHolder> phs = new List<ContentPlaceHolder>();
            var allPhs = searchRoot.GetDescendantControls<ContentPlaceHolder>();

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
        

        public void ActivateAndPlaceAdHocPageContent(Page page, List<CmsPageContent> allContent)
        {
            var leaves = IdentifyLayoutLeaves(page);

            var pageRoot = page.GetPageRoot();
            ActivateAndPlaceContent(pageRoot, allContent, _context.PageRenderMode);

            if (_context.PageRenderMode == PageRenderMode.PageDesigner)
            {
                foreach (var leaf in leaves)
                    leaf.Controls.AddAt(0, new DropTarget(leaf, DropTargetDirective.Begin));

                foreach (var leaf in leaves)
                    leaf.Controls.Add(new DropTarget(leaf, DropTargetDirective.End));
            }
        }



        public void ActivateAndPlaceLayoutContent(Page localPage)
        {
            localPage.MasterPageFile = "/App_Data/AdHocLayout.master";
            if (_context.CmsPage.LayoutId == Guid.Empty)
                return;

            var layoutToApply = layoutRepository.GetById(_context.CmsPage.LayoutId);
            var structure = layoutRepository.GetLayoutStructure(layoutToApply);
            var lns = FlattenLayoutTree(structure);

            if (lns.Any())
            {
                localPage.MasterPageFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
            }

            var root = localPage.GetPageRoot();
            foreach (var ln in lns)
                ActivateAndPlaceContent(root, ln.Layout.PageContent, PageRenderMode.Readonly);
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
                    pageBuilder.ActivateAndPlaceLayoutContent(localPage);

                    var allContent = rt.CmsPage.PageContent;
                    if (rt.PageRenderMode == PageRenderMode.PageDesigner)
                    {
                        var editing = new EditingContextManager();

                        ////todo: see if this can get better once content routing is in place.
                        //var designMode = HttpContext.Current.Request["wc-editor"];
                        //if ("form" == designMode)
                        //{
                        //    var formIdRaw = HttpContext.Current.Request["wc-form-id"];
                        //    if (string.IsNullOrWhiteSpace(formIdRaw))
                        //    {
                        //        var blankForm = new CmsForm {ContentId = Guid.NewGuid()};
                        //        var context = editing.GetOrCreateEditingContext(blankForm);
                        //        allContent = context.AllContent;
                        //    }
                        //    else
                        //    {
                        //        var formId = new Guid(formIdRaw);
                        //        var form = new FormRepository().FindContentVersions(By.ContentId(formId),ContentEnvironment.Draft).Result.Single();
                        //        var context = editing.GetOrCreateEditingContext(form);
                        //        allContent = context.AllContent;
                        //    }
                        //}
                        //else
                        //{
                            var context = editing.GetOrCreateEditingContext(rt.CmsPage);
                            allContent = context.AllContent;
                        //}
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