using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms;
using Cms.Layout;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets;

namespace WarpCore.Web
{
    public class CmsPageLayoutEngine
    {
        private readonly CmsPageRequestContext _context;
        private readonly LayoutRepository layoutRepository = new LayoutRepository();
        public CmsPageLayoutEngine(CmsPageRequestContext context)
        {
            _context = context;
        }



        public static IReadOnlyCollection<Control> ActivateAndPlaceContent(Control placementSearchContext, IReadOnlyCollection<CmsPageContent> contents, PageRenderMode vm = PageRenderMode.Readonly)
        {
            List<Control> activatedControls = new List<Control>();

            int i = 0;
            foreach (var content in contents)
            {
                i++;
                var activatedWidget = CmsPageContentActivator.ActivateControl(content);

                var layoutWidget = activatedWidget as LayoutControl;
                if (layoutWidget != null)
                {
                    layoutWidget.ID = $"Layout{i}";
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
            uc.FriendlyName = toolboxItem.FriendlyName;
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

            if (ph == null)
                ph = searchContext.FindDescendantControlOrSelf<RuntimeContentPlaceHolder>(x => true);

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
}