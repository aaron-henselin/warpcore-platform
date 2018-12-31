using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms;
using Cms.Layout;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Web.Extensions;
using WarpCore.Web.RenderingEngines.WebForms;
using WarpCore.Web.Widgets;
using StringBuilder = System.Text.StringBuilder;

namespace WarpCore.Web
{
    public class PageRendering
    {
        public PartialPageRendering Rendering { get; set; }
        public List<string> Scripts { get; set; }
        public List<string> Styles { get; set; }

        public Dictionary<Guid, PartialPageRendering> GetPartialPageRenderingByLayoutBuilderId()
        {
            Dictionary<Guid,PartialPageRendering> d = new Dictionary<Guid, PartialPageRendering>();
            
            d = Rendering.GetAllDescendents().Where(x => x.LayoutBuilderId != Guid.Empty).ToDictionary(x => x.LayoutBuilderId);
            d.Add(WebFormsRenderEngine.LayoutBuilderIds.PageRoot, Rendering);

            return d;
        }


       
    }

    public class UndefinedLayoutPartialPageRendering : PartialPageRendering
    {
        public UndefinedLayoutPartialPageRendering()
        {
            PlaceHolders.Add("Body",new RenderingsPlaceHolder {Id="Body"});
        }
    }

    public class PartialPageRendering 
    {
        public Guid ContentId { get; set; }
        public string LocalId { get; set; }
        public Guid LayoutBuilderId { get; set; }
        public bool IsFromLayout { get; set; }
        public string FriendlyName { get; set; }
        public Dictionary<string,RenderingsPlaceHolder> PlaceHolders { get; } = new Dictionary<string,RenderingsPlaceHolder>();
        public List<string> GlobalPlaceHolders { get; } = new List<string>();


        public IReadOnlyCollection<PartialPageRendering> GetAllDescendents()
        {
            return GetAllDescendents(this);
        }

        private IReadOnlyCollection<PartialPageRendering> GetAllDescendents(PartialPageRendering parent)
        {
            List<PartialPageRendering> partials = new List<PartialPageRendering>();

            partials.Add(parent);

            foreach (var ph in parent.PlaceHolders.Values)
            foreach (var child in ph.Renderings)
            {
                var desc = GetAllDescendents(child);
                partials.AddRange(desc);
            }

            return partials;
        }
    }

    public class LiteralPartialPageRendering : PartialPageRendering
    {
        
        public LiteralPartialPageRendering()
        {
            ContentId = Guid.NewGuid();
        }

        public LiteralPartialPageRendering(string text):this()
        {
            Text = text;
        }

        public string Text { get; set; }
    }



    public class WebFormsPageRendering : PartialPageRendering, IHandledByWebFormsRenderingEngine
    {
        private readonly Page _masterPage;

        public WebFormsPageRendering(Page masterPage)
        {
            _masterPage = masterPage;
            var placeHolders = _masterPage.GetRootControl().GetDescendantControls<ContentPlaceHolder>();
            foreach (var nativePlaceHolder in placeHolders)
                this.PlaceHolders.Add(nativePlaceHolder.ID,new RenderingsPlaceHolder {Id = nativePlaceHolder.ID});

            this.GlobalPlaceHolders.Add(GlobalLayoutPlaceHolderIds.Head);
            this.GlobalPlaceHolders.Add(GlobalLayoutPlaceHolderIds.Scripts);

            IsFromLayout = true;
        }

        public Page GetPage()
        {
            return _masterPage;
        }
    }

    public interface IHandledByWebFormsRenderingEngine
    {
    }

    public class WebFormsWidget : PartialPageRendering, IHandledByWebFormsRenderingEngine
    {
        private Control activatedWidget;

        public WebFormsWidget(Control activatedWidget, Guid contentId)
        {
            ContentId = contentId;
            this.activatedWidget = activatedWidget;

            this.LocalId = activatedWidget.ID;
            var layout = this.activatedWidget as ILayoutControl;
            if (layout != null)
                this.LayoutBuilderId = layout.LayoutBuilderId;
        }
        

        public Control GetControl()
        {
            return activatedWidget;
        }
    }

    public class CompositableContent
    {
        
        public Dictionary<Guid,List<ITransformOutput>> WidgetContent { get; set; } = new Dictionary<Guid, List<ITransformOutput>>();
        public Dictionary<string, List<string>> GlobalContent { get; set; } = new Dictionary<string, List<string>>();
    }

    public class GlobalSubstitutionOutput : ITransformOutput
    {
        public string Id { get; set; }
    }

    public class LayoutSubstitutionOutput : ITransformOutput
    {
        public string Id { get; set; }
    }

    public class HtmlOutput : ITransformOutput
    {
        public string sb;

        public HtmlOutput(StringBuilder sb)
        {
            this.sb = sb.ToString();
        }
    }

    public class BeginWidgetHtmlOutput : HtmlOutput
    {
        public BeginWidgetHtmlOutput(StringBuilder sb) : base(sb)
        {
        }
    }

    public class EndWidgetHtmlOutput : HtmlOutput
    {
        public EndWidgetHtmlOutput(StringBuilder sb) : base(sb)
        {
        }
    }

    public interface ITransformOutput
    {
    }



    public class CompositedPage
    {
        public string Html { get; set; }
    }

    public class CompositeRenderingEngine
    {
        public CompositedPage Execute(PageRendering pageRendering,PageRenderMode renderMode)
        {
            var transformationResult = new CompositableContent();

            var webForms = new WebFormsRenderEngine();


            var batch = webForms.Execute(pageRendering.Rendering);

            var allDescendents = pageRendering.Rendering.GetAllDescendents();

            var literals = allDescendents.OfType<LiteralPartialPageRendering>().ToList();
            foreach (var literal in literals)
            {
                var htmlOutput = new HtmlOutput(new StringBuilder(literal.Text));
                batch.WidgetContent.Add(literal.ContentId,new List<ITransformOutput> {htmlOutput});
            }

            
            
            var compositor = new PageCompositor.PageCompositor(pageRendering, batch);
            return compositor.Composite(renderMode);
        }


    }

    public interface IBatchingRenderEngine
    {
    }

    public static class GlobalLayoutPlaceHolderIds
    {
        public const string Head = "__HEAD";
        public const string Scripts = "__SCRIPTS";

    }

    public class CmsPageLayoutEngine
    {
        private readonly CmsPageRequestContext _context;
        private readonly LayoutRepository layoutRepository = new LayoutRepository();
        public CmsPageLayoutEngine(CmsPageRequestContext context)
        {
            _context = context;
        }


        public static void ActivateAndPlaceContent(PartialPageRendering parentRendering,
            IReadOnlyCollection<CmsPageContent> contents, bool isFromLayout)
        {
            //List<PartialPageRendering> activatedControls = new List<PartialPageRendering>();


            int i = 0;
            foreach (var content in contents)
            {
                i++;

                var activatedWidget = CmsPageContentActivator.ActivateControl(content);
                activatedWidget.IsFromLayout = isFromLayout;

                var placementPlaceHolder = FindPlacementLocation(parentRendering, content);
                if (placementPlaceHolder == null)
                    continue;

                if (content.AllContent.Any())
                {
                    ActivateAndPlaceContent(activatedWidget, content.AllContent, isFromLayout);
                    //activatedControls.AddRange(subCollection);
                }

                placementPlaceHolder.Renderings.Add(activatedWidget);
            }
        }
    

        private static void AddLayoutHandle(RenderingsPlaceHolder ph, CmsPageContent content)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(content.WidgetTypeCode);
            var ascx = BuildManager.GetCompiledType("/App_Data/PageDesignerComponents/LayoutHandle.ascx");
            var uc = (ILayoutHandle)Activator.CreateInstance(ascx);
            uc.HandleName = toolboxItem.WidgetUid;
            uc.PageContentId = content.Id;
            uc.FriendlyName = toolboxItem.FriendlyName;

            var fake = Guid.NewGuid();
            ph.Renderings.Add(new WebFormsWidget((Control)uc,fake));
        }

        private static RenderingsPlaceHolder FindPlacementLocation(PartialPageRendering searchContext, CmsPageContent content)
        {
            try
            {
                return searchContext.PlaceHolders[content.PlacementContentPlaceHolderId];
            }
            catch (KeyNotFoundException)
            {
                return searchContext.PlaceHolders.FirstOrDefault().Value; //should this be ordered?
            }
            
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
        

        public void ActivateAndPlaceAdHocPageContent(PartialPageRendering page, CmsPageContent item)
        {
            ActivateAndPlaceContent(page,new[]{item},false);

            //var leaves = IdentifyLayoutLeaves(page);
            //var rootControl = page.GetRootControl();

            //var root = new WebFormsControlRendering(rootControl){};
            
            //foreach (var content in allContent)
            //    ActivateAndPlaceContent(root, allContent, _context.PageRenderMode);

            //if (_context.PageRenderMode == PageRenderMode.PageDesigner)
            //{
            //    foreach (var leaf in leaves)
            //        leaf.Controls.AddAt(0, new DropTarget(leaf, DropTargetDirective.Begin));

            //    foreach (var leaf in leaves)
            //        leaf.Controls.Add(new DropTarget(leaf, DropTargetDirective.End));
            //}
        }

        public void ActivateAndPlaceLayoutContent(PageRendering page)
        {
            page.Rendering = new UndefinedLayoutPartialPageRendering();
            if (_context.CmsPage.LayoutId == Guid.Empty)
                return;
            
            
            var layoutToApply = layoutRepository.GetById(_context.CmsPage.LayoutId);
           
            
            if (!string.IsNullOrWhiteSpace(layoutToApply.MasterPagePath))
                page.Rendering = CmsPageContentActivator.ActivateLayout(layoutToApply.MasterPagePath);
            

            var structure = layoutRepository.GetLayoutStructure(layoutToApply);
            var lns = FlattenLayoutTree(structure);

            if (lns.Any())
            {
                var mpFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
                page.Rendering = CmsPageContentActivator.ActivateLayout(mpFile);
            }
            

            //var root = localPage.GetRootControl();
            foreach (var ln in lns)
                ActivateAndPlaceContent(page.Rendering, ln.Layout.PageContent, true);
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