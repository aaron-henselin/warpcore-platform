using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

    public abstract class EditingCommand
    {
        public string ToContentPlaceHolderId { get; set; }
        public Guid? ToLayoutBuilderId { get; set; }
        public Guid? BeforePageContentId { get; set; }
    }

    public class AddCommand : EditingCommand
    {
        public string WidgetType { get; set; }

    }

    public class MoveCommand : EditingCommand
    {
        public Guid PageContentId { get; set; }
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
                var toolboxPassthrough = page.Request["WC_TOOLBOX_STATE"] ?? string.Empty;

                var editingContext = GetEditingContext();
                var editingContextJson = _js.Serialize(editingContext);
                var htmlEncoded = page.Server.HtmlEncode(editingContextJson);
                var lit = new Literal { };
                lit.Text =
                $@"
<input name='WC_EDITING_CONTEXT_JSON' value='{htmlEncoded}'/>
<input id='WC_EDITING_MOVE_COMMAND' name='WC_EDITING_MOVE_COMMAND' value=''/>
<input id='WC_EDITING_ADD_COMMAND' name='WC_EDITING_ADD_COMMAND' value=''/>
<input id='WC_TOOLBOX_STATE' name='WC_TOOLBOX_STATE' value='{page.Server.HtmlEncode(toolboxPassthrough)}'/>

";
                page.Form.Controls.Add(lit);
                //page.Form.Controls.Add(new HtmlInputHidden { ClientIDMode = ClientIDMode.Static, Name= "", ID = "WC_EDITING_CONTEXT_JSON",Value= editingContextJson });
                //page.Form.Controls.Add(new HtmlInputHidden { ClientIDMode = ClientIDMode.Static, ID = "WC_EDITING_MOVE_COMMAND",Value=""});
                page.Form.Controls.Add(new Button { ClientIDMode = ClientIDMode.Static, ID = "WC_EDITING_SUBMIT" });
            };

        }

        public EditingContext GetOrCreateEditingContext(CmsPage cmsPage)
        {
            

            if (HttpContext.Current.Request["WC_EDITING_CONTEXT_JSON"] == null)
            {
                var raw = _js.Serialize(new EditingContext {SubContent = cmsPage.PageContent});
                HttpContext.Current.Items["WC_EDITING_CONTEXT"] = _js.Deserialize<EditingContext>(raw);
            }
            else
            {
                var json = HttpContext.Current.Request["WC_EDITING_CONTEXT_JSON"];
                HttpContext.Current.Items["WC_EDITING_CONTEXT"] = _js.Deserialize<EditingContext>(json);
            }

            return GetEditingContext();
        }

        private void X()
        {

        }

        public void ProcessAddCommand(EditingContext editingContext,AddCommand addCommand)
        {
            var newContent = new CmsPageContent
            {
                PlacementContentPlaceHolderId = addCommand.ToContentPlaceHolderId,
                PlacementLayoutBuilderId = addCommand.ToLayoutBuilderId,
                WidgetTypeCode = addCommand.WidgetType,
                Parameters = new Dictionary<string, string>()
            };

            if (addCommand.ToLayoutBuilderId != null)
            {
                var newParentSearch =
                    editingContext.FindSubContentReursive(x =>
                        x.Parameters.ContainsKey(nameof(LayoutControl.LayoutBuilderId)) &&
                        new Guid(x.Parameters[nameof(LayoutControl.LayoutBuilderId)]) ==
                        addCommand.ToLayoutBuilderId.Value).Single();

                //todo: ordering.

                newParentSearch.LocatedContent.SubContent.Add(newContent);
            }
            else
            {
                //todo: ordering.

                editingContext.SubContent.Add(newContent);
            }
        }

        public void ProcessMoveCommand(EditingContext editingContext, MoveCommand moveCommand)
        {
            var contentToMoveSearch = editingContext.FindSubContentReursive(x => x.Id == moveCommand.PageContentId).SingleOrDefault();
            if (contentToMoveSearch == null)
                throw new Exception("component not found.");

            contentToMoveSearch.ParentContent.SubContent.Remove(contentToMoveSearch.LocatedContent);

            if (moveCommand.ToLayoutBuilderId != null)
            {
                var newParentSearch =
                    editingContext.FindSubContentReursive(x =>
                        x.Parameters.ContainsKey(nameof(LayoutControl.LayoutBuilderId)) &&
                        new Guid(x.Parameters[nameof(LayoutControl.LayoutBuilderId)]) ==
                        moveCommand.ToLayoutBuilderId.Value).Single();

                //todo: ordering.

                newParentSearch.LocatedContent.SubContent.Add(contentToMoveSearch.LocatedContent);
            }
            else
            {
                //todo: ordering.

                editingContext.SubContent.Add(contentToMoveSearch.LocatedContent);
            }

            contentToMoveSearch.LocatedContent.PlacementContentPlaceHolderId = moveCommand.ToContentPlaceHolderId;
            contentToMoveSearch.LocatedContent.PlacementLayoutBuilderId = moveCommand.ToLayoutBuilderId;

        }

        public EditingContext GetEditingContext()
        {
            return (EditingContext)HttpContext.Current.Items["WC_EDITING_CONTEXT"];
        }


        public void ProcessEditingCommands(EditingContext editingContext)
        {
            var moveCommandRaw = HttpContext.Current.Request["WC_EDITING_MOVE_COMMAND"];
            if (!string.IsNullOrWhiteSpace(moveCommandRaw))
            {
                var realMoveCommand = _js.Deserialize<MoveCommand>(moveCommandRaw);
                ProcessMoveCommand(editingContext, realMoveCommand);
            }

            var addCommandRaw = HttpContext.Current.Request["WC_EDITING_ADD_COMMAND"];
            if (!string.IsNullOrWhiteSpace(addCommandRaw))
            {
                var addCommand = _js.Deserialize<AddCommand>(addCommandRaw);
                ProcessAddCommand(editingContext, addCommand);
            }
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
            var handle = new LayoutHandle {PageContentId = content.Id, HandleName = toolboxItem.Name};
            ph.Controls.Add(handle);
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
                .SingleOrDefault(x => x.ID == content.PlacementContentPlaceHolderId);
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
                editing.ProcessEditingCommands(context);
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

    public class LayoutHandle : PlaceHolder
    {
        public Guid PageContentId { get; set; }
        public string HandleName { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var literal = new Literal();
            literal.Text = $@"
<li class=""StackedListItem StackedListItem--isDraggable wc-layout-handle"" tabindex=""1""  data-wc-page-content-id=""{PageContentId}"" >
<div class=""StackedListContent"">
<h4 class=""Heading Heading--size4 text-no-select"">{HandleName}

<span class='pull-right' >X</span>
</h4>
<div class=""DragHandle""></div>
<div class=""Pattern Pattern--typeHalftone""></div>
<div class=""Pattern Pattern--typePlaced""></div></div></li>
            ";

            //var p = new Panel();
            //p.Attributes["data-wc-role"] = "layout-handle";
            //p.Attributes["data-wc-page-content-id"] = PageContentId.ToString();
            //p.Attributes["class"] = "wc-layout-handle";
            //p.Controls.Add(new Label{Text=HandleName});
            this.Controls.Add(literal);
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