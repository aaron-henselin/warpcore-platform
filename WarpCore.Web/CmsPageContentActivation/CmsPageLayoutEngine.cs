using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WarpCore.Platform.Kernel;
using WarpCore.Web.Extensions;
using WarpCore.Web.RenderingEngines.WebForms;
using WarpCore.Web.Widgets;
using StringBuilder = System.Text.StringBuilder;

namespace WarpCore.Web
{
    public class PageRenderingDirective
    {
        public PartialPageRendering Rendering { get; set; }
        public List<string> Scripts { get; set; }
        public List<string> Styles { get; set; }

        public Dictionary<Guid, PartialPageRendering> GetPartialPageRenderingByLayoutBuilderId()
        {
            Dictionary<Guid,PartialPageRendering> d = new Dictionary<Guid, PartialPageRendering>();
            
            d = Rendering.GetAllDescendents().Where(x => x.LayoutBuilderId != Guid.Empty).ToDictionary(x => x.LayoutBuilderId);
            d.Add(SpecialRenderingFragmentContentIds.PageRoot, Rendering);

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

    public static class KnownPhysicalFileExtensions
    {
        public static string MasterPage = "master";
        public static string Razor = "cshtml";
    }

    public interface IPartialPageRenderingFactory
    {
        object ActivateType(Type type);

        IReadOnlyCollection<Type> GetHandledBaseTypes();

        IReadOnlyCollection<string> GetHandledFileExtensions();

        PartialPageRendering CreateRenderingForObject(object nativeWidgetObject);

        PartialPageRendering CreateRenderingForPhysicalFile(string physicalFilePath);
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


    public class RenderingFragmentCollection
    {
        public Dictionary<Guid,List<IRenderingFragment>> WidgetContent { get; set; } = new Dictionary<Guid, List<IRenderingFragment>>();
        public Dictionary<string, List<string>> GlobalContent { get; set; } = new Dictionary<string, List<string>>();
    }

    [DebuggerDisplay("GlobalPlaceHolder = {" + nameof(Id) + "}")]
    public class GlobalSubstitutionOutput : IRenderingFragment
    {
        public string Id { get; set; }
    }

    [DebuggerDisplay("LayoutPlaceHolder = {" + nameof(Id) + "}")]
    public class LayoutSubstitutionOutput : IRenderingFragment
    {
        public string Id { get; set; }
    }

    [DebuggerDisplay("Html = {" + nameof(Html) + "}")]
    public class HtmlOutput : IRenderingFragment
    {
        public string Html;

        public HtmlOutput(StringBuilder sb)
        {
            this.Html = sb.ToString();
        }
    }
    

    public interface IRenderingFragment
    {
    }



    public class CompositedPage
    {
        public string Html { get; set; }
    }

    public static class SpecialRenderingFragmentContentIds
    {
        public static Guid PageRoot = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        public static Guid WebFormsInterop = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
    }

    public class CompositeRenderingEngine
    {
        public CompositedPage Execute(PageRenderingDirective pageRendering,PageRenderMode renderMode)
        {
            var transformationResult = new RenderingFragmentCollection();

            var webForms = new WebFormsRenderEngine();


            var batch = webForms.Execute(pageRendering.Rendering);

            var allDescendents = pageRendering.Rendering.GetAllDescendents();

            var literals = allDescendents.OfType<LiteralPartialPageRendering>().ToList();
            foreach (var literal in literals)
            {
                var htmlOutput = new HtmlOutput(new StringBuilder(literal.Text));
                batch.WidgetContent.Add(literal.ContentId,new List<IRenderingFragment> {htmlOutput});
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
        public const string InternalStateTracking = "__FORMDESIGNER";
    }

    public class CmsPageLayoutEngine
    {
        private readonly CmsPageRequestContext _context;
        private readonly LayoutRepository layoutRepository = new LayoutRepository();
        private CmsPageContentActivator _contentActivator;

        public CmsPageLayoutEngine(): this(Dependency.Resolve<CmsPageRequestContext>(),Dependency.Resolve<CmsPageContentActivator>())
        {
           
        }


        public CmsPageLayoutEngine(CmsPageRequestContext context, CmsPageContentActivator contentActivator)
        {
            _context = context;
            _contentActivator = contentActivator;
        }




        private void ActivateAndPlaceContent(PartialPageRendering parentRendering,
            IReadOnlyCollection<CmsPageContent> contents, bool isFromLayout)
        {
            int i = 0;
            foreach (var content in contents)
            {
                i++;

                var activatedWidget = _contentActivator.ActivateCmsPageContent(content);
                activatedWidget.IsFromLayout = isFromLayout;

                var placementPlaceHolder = FindPlacementLocation(parentRendering, content);
                if (placementPlaceHolder == null)
                    continue;

                if (content.AllContent.Any())
                    ActivateAndPlaceContent(activatedWidget, content.AllContent, isFromLayout);

                placementPlaceHolder.Renderings.Add(activatedWidget);
            }
        }
    

        //private static void AddLayoutHandle(RenderingsPlaceHolder ph, CmsPageContent content)
        //{
        //    var toolboxItem = new ToolboxManager().GetToolboxItemByCode(content.WidgetTypeCode);
        //    var ascx = BuildManager.GetCompiledType("/App_Data/PageDesignerComponents/LayoutHandle.ascx");
        //    var uc = (ILayoutHandle)Activator.CreateInstance(ascx);
        //    uc.HandleName = toolboxItem.WidgetUid;
        //    uc.PageContentId = content.Id;
        //    uc.FriendlyName = toolboxItem.FriendlyName;

        //    var fake = Guid.NewGuid();
        //    ph.Renderings.Add(new WebFormsControlPartialPageRendering((Control)uc,fake));
        //}

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

        //public IReadOnlyCollection<ContentPlaceHolder> IdentifyLayoutLeaves(Control searchRoot)
        //{
        //    List<ContentPlaceHolder> phs = new List<ContentPlaceHolder>();
        //    var allPhs = searchRoot.GetDescendantControls<ContentPlaceHolder>();

        //    foreach (var ph in allPhs)
        //    {
        //        var isLeaf = !ph.GetDescendantControls<ContentPlaceHolder>().Any();
        //        if (isLeaf)
        //        {
        //            phs.Add(ph);
        //        }
        //    }

        //    return phs;
        //}

        public void ActivateAndPlaceAdHocPageContent(CmsPageContent contentToActivate, PartialPageRendering page)
        {
            ActivateAndPlaceContent(page,new[]{contentToActivate},false);
        }

        public void ActivateAndPlaceLayoutContent(IReadOnlyCollection<CmsPageContent> contentToActivate, PartialPageRendering parentRendering)
        {
            ActivateAndPlaceContent(parentRendering, contentToActivate, true);
        }

        public void ActivateAndPlaceLayoutContent(PageRenderingDirective page)
        {
            page.Rendering = new UndefinedLayoutPartialPageRendering();
            if (_context.CmsPage.LayoutId == Guid.Empty)
                return;
            
            
            var layoutToApply = layoutRepository.GetById(_context.CmsPage.LayoutId);
           
            
            if (!string.IsNullOrWhiteSpace(layoutToApply.MasterPagePath))
                page.Rendering = _contentActivator.ActivateLayoutByExtension(layoutToApply.MasterPagePath);
            

            var structure = layoutRepository.GetLayoutStructure(layoutToApply);
            var lns = FlattenLayoutTree(structure);

            if (lns.Any())
            {
                var mpFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
                page.Rendering = _contentActivator.ActivateLayoutByExtension(mpFile);
            }
            

            //var root = localPage.GetRootControl();
            foreach (var ln in lns)
                ActivateAndPlaceLayoutContent(ln.Layout.PageContent, page.Rendering);
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