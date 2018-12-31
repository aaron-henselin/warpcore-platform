using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Cms;
using Cms.Layout;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Web.Extensions;
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

            this.PlaceHolders.Add(GlobalLayoutPlaceHolderIds.Head,new RenderingsPlaceHolder{Id= GlobalLayoutPlaceHolderIds.Head });
            this.PlaceHolders.Add(GlobalLayoutPlaceHolderIds.Scripts, new RenderingsPlaceHolder { Id = GlobalLayoutPlaceHolderIds.Scripts });

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
    }

    public class SubstitutionOutput : ITransformOutput
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

    public class PageCompositor
    {
        private readonly PageRendering _pageDefinition;
        private readonly CompositableContent _contentToComposite;

        public PageCompositor(PageRendering pageDefinition, CompositableContent contentToComposite)
        {
            _pageDefinition = pageDefinition;
            _contentToComposite = contentToComposite;
        }

        public CompositedPage Composite(PageRenderMode renderMode)
        {
            var page = new CompositedPage();
            var sb = new StringBuilder();
            Render(_pageDefinition.Rendering, renderMode, sb);
            page.Html = sb.ToString();
            return page;
        }

        private void Render(PartialPageRendering pp,PageRenderMode renderMode, StringBuilder local)
        {
            var parts = _contentToComposite.WidgetContent[pp.ContentId];

            for (var index = 0; index < parts.Count; index++)
            {
                var part = parts[index];
                var renderDesignElements = !pp.IsFromLayout && renderMode == PageRenderMode.PageDesigner;

                if (part is HtmlOutput)
                {
                    if (renderDesignElements && index == 0)
                    {
                        var layoutHandle = $@"
                            <li class='StackedListItem StackedListItem--isDraggable wc-layout-handle' tabindex='1'
                                data-wc-page-content-id='{pp.ContentId}'>
                                <div class='StackedListContent'>
                                    <h4 class='Heading Heading--size4 text-no-select'>

                                        <span class='glyphicon glyphicon-cog wc-edit-command configure'
                                            data-wc-widget-type='<%# HandleName %>' 
                                            data-wc-editing-command-configure='{pp.ContentId}'>
                                        </span>
                                        <span class='glyphicon glyphicon-remove wc-edit-command delete pull-right' 
                                             data-wc-editing-command-delete='{pp.ContentId}'>
                                        </span>
                                        
                                        {pp.FriendlyName}
                                    </h4>
                                    <div class='DragHandle'></div>
                                    <div class='Pattern Pattern--typeHalftone'></div>
                                    <div class='Pattern Pattern--typePlaced'></div>
                                </div>
                            </li>
                        ";
                        local.Append(layoutHandle);
                        local.Append(
                            $"<wc-widget-render data-wc-layout='{pp.PlaceHolders.Any()}' data-wc-page-content-id='{pp.ContentId}'>");
                    }

                    local.Append(((HtmlOutput) part).sb);

                    if (renderDesignElements && index == parts.Count-1)
                    {
                        local.Append("</wc-widget-render>");
                    }
                }

                if (part is SubstitutionOutput)
                {
                    var subPart = (SubstitutionOutput) part;
                    var relevantPlaceHolder = pp.PlaceHolders[subPart.Id];

                    if (renderDesignElements)
                        local.Append(
                            $"<wc-droptarget data-wc-placeholder-id='{relevantPlaceHolder.Id}' data-wc-layout-builder-id='{pp.LayoutBuilderId}'>");

                    foreach (var item in relevantPlaceHolder.Renderings)
                        Render(item, renderMode, local);

                    if (renderDesignElements)
                        local.Append("</wc-droptarget>");
                }
            }
        }
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

            
            
            var compositor = new PageCompositor(pageRendering, batch);
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

    public class WebFormsRenderEngine : IBatchingRenderEngine
    {
        private class SwitchingHtmlWriter : StringWriter
        {
            private Stack<Guid> _idStack = new Stack<Guid>();

            public Dictionary<Guid,List<ITransformOutput>> output = new Dictionary<Guid, List<ITransformOutput>>();

            

            public void BeginWriting(Guid id)
            {
                if (_idStack.Count > 0)
                {
                    var sb = this.GetStringBuilder();
                    if (!output[_idStack.Peek()].Any())
                        output[_idStack.Peek()].Add(new BeginWidgetHtmlOutput(sb));
                    else
                        output[_idStack.Peek()].Add(new HtmlOutput(sb));

                    sb.Clear();
                }

                _idStack.Push(id);
                output.Add(id, new List<ITransformOutput>());

            }

            public void AddSubsitution(string id)
            {
                var sb = this.GetStringBuilder();
                output[_idStack.Peek()].Add(new HtmlOutput(sb));
                sb.Clear();

                output[_idStack.Peek()].Add(new SubstitutionOutput{Id = id});
            }

            public void EndWriting()
            {
                var id = _idStack.Pop();

                var sb = this.GetStringBuilder();
                output[id].Add(new EndWidgetHtmlOutput(sb));
                sb.Clear();
              
            }
            

        }

        private class SubstitutionComponent : Control
        {
            private readonly RenderingsPlaceHolder _ph;

            public SubstitutionComponent(RenderingsPlaceHolder ph)
            {
                _ph = ph;
            }

            protected override void Render(HtmlTextWriter writer)
            {

                var switching = (SwitchingHtmlWriter)writer.InnerWriter;
                switching.AddSubsitution(_ph.Id);
            }
        }

        private class RenderingEngineComponent : PlaceHolder
        {
            private readonly Guid _id;

            public RenderingEngineComponent(Control control, WebFormsWidget pp)
            {
                _id = pp.ContentId;
                Controls.Add(control);

                foreach (var ph in pp.PlaceHolders.Values)
                {
                   var contentPlaceHolder = control.FindControl(ph.Id);
                   contentPlaceHolder.Controls.Add(new SubstitutionComponent(ph));
                }

            }

            protected override void Render(HtmlTextWriter writer)
            {
                
                var switching = (SwitchingHtmlWriter)writer.InnerWriter;
                switching.BeginWriting(_id);
                base.Render(writer);
                switching.EndWriting();
            }
        }

        private class NonWebFormsControl : Control
        {
          
        }

        private static void BuildServerSidePage(Control nativeRoot, PartialPageRendering pp)
        {
            foreach (var kvp in pp.PlaceHolders)
            {
                var placeHolder = kvp.Value;

                Control contentPlaceHolder= nativeRoot.FindControl(placeHolder.Id);

                if (contentPlaceHolder == null)
                    throw new Exception("Placeholder " + placeHolder.Id + " does not exist.");

                foreach (var placedRendering in placeHolder.Renderings)
                {
                    if (placedRendering is WebFormsWidget)
                    {
                        WebFormsWidget webFormsRendering = ((WebFormsWidget) placedRendering);
                        var control = webFormsRendering.GetControl();
                        contentPlaceHolder.Controls.Add(new RenderingEngineComponent(control,(WebFormsWidget)placedRendering));

                        BuildServerSidePage(control, placedRendering);
                    }
                    else
                    {
                        var control = new NonWebFormsControl {ID=placedRendering.LocalId};
                        foreach (var nonWebFormsPlaceHolder in placedRendering.PlaceHolders.Values)
                            control.Controls.Add(new NonWebFormsControl {ID = nonWebFormsPlaceHolder.Id});

                        contentPlaceHolder.Controls.Add(control);
                        BuildServerSidePage(control, placedRendering);
                    }
                }
            }
        }

        public static class LayoutBuilderIds
        {
            public static Guid PageRoot = Guid.Empty;
            public static Guid WebFormsInterop = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        }


        public CompositableContent Execute(PartialPageRendering pp)
        {

            SwitchingHtmlWriter _writer = new SwitchingHtmlWriter();
            
            
            var isWebFormsInChargeOfPageBase = pp is WebFormsPageRendering;
            if (isWebFormsInChargeOfPageBase)
            {
                var nativePageRendering = (WebFormsPageRendering)pp;
                var nativePage = nativePageRendering.GetPage();
                var topMostControl = nativePage.GetRootControl();
                
                //nativePage.Header.Controls.Add(new WebFormsWidget());

                foreach (var ph in pp.PlaceHolders.Values)
                {
                    if (new[] {GlobalLayoutPlaceHolderIds.Head, GlobalLayoutPlaceHolderIds.Scripts}.Contains(ph.Id))
                        continue;

                    var contentPlaceHolder = topMostControl.FindControl(ph.Id);
                    contentPlaceHolder.Controls.Add(new SubstitutionComponent(ph));
                }


                BuildServerSidePage(topMostControl, pp);

                //allows nonwebforms controls to get access to the head and the scripts
                nativePage.InitComplete += (sender, args) =>
                {
                    if (nativePage.Header == null)
                        throw new Exception("Add a <head runat=server> tag in order to use this master page as a layout.");

                    if (nativePage.Form == null)
                        throw new Exception("Add a <form runat=server> tag in order to use this master page as a layout.");


                    nativePage.Header.Controls.Add(new SubstitutionComponent(new RenderingsPlaceHolder() { Id = GlobalLayoutPlaceHolderIds.Head }));
                    nativePage.Form.Controls.Add(new SubstitutionComponent(new RenderingsPlaceHolder() { Id = GlobalLayoutPlaceHolderIds.Scripts }));

                };
              
               _writer.BeginWriting(pp.ContentId);
                HttpContext.Current.Server.Execute(nativePage, _writer, true);
                _writer.EndWriting();
            }
            else
            {
                var nativeRoot = new Page();
                var body = new HtmlGenericControl("body");
                var form = new HtmlGenericControl("form");


                var wrapper = new RenderingEngineComponent(body,new WebFormsWidget(body,LayoutBuilderIds.WebFormsInterop));
                wrapper.Controls.Add(body);
                body.Controls.Add(form);
                nativeRoot.Controls.Add(wrapper);

                foreach (var placeholder in pp.PlaceHolders)
                    form.Controls.Add(new ContentPlaceHolder {ID=placeholder.Value.Id});

                BuildServerSidePage(nativeRoot,pp);
                HttpContext.Current.Server.Execute(nativeRoot, _writer, true);
            }

            
            

            return new CompositableContent
            {
                WidgetContent = _writer.output,


            };
        }

        private void NativePage_InitComplete(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
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

        //private static Control FindPlacementLocation(Control searchContext,CmsPageContent content)
        //{

        //    if (content.PlacementLayoutBuilderId != null)
        //    {
        //        var subLayout =
        //            searchContext.FindDescendantControlOrSelf<LayoutControl>(x =>
        //                x.LayoutBuilderId == content.PlacementLayoutBuilderId);
                
        //        if (subLayout != null)
        //            searchContext = subLayout;
        //    }

        //    Control ph;

        //    ph = searchContext.FindDescendantControlOrSelf<ContentPlaceHolder>(x =>
        //        x.ID == content.PlacementContentPlaceHolderId);

        //    if (ph == null)
        //        ph = searchContext.FindDescendantControlOrSelf<RuntimeContentPlaceHolder>(x =>
        //            x.PlaceHolderId == content.PlacementContentPlaceHolderId);

        //    if (ph == null)
        //        ph = searchContext.FindDescendantControlOrSelf<ContentPlaceHolder>(x => true);

        //    if (ph == null)
        //        ph = searchContext.FindDescendantControlOrSelf<RuntimeContentPlaceHolder>(x => true);

        //    return ph;
        //}

        //public class DropTarget : PlaceHolder
        //{
        //    private readonly string _directive;


        //    public DropTarget()
        //    {
        //    }

        //    public DropTarget(ContentPlaceHolder leaf, DropTargetDirective directive)
        //    {
        //        _directive = directive.ToString();
        //        PlaceHolderId = leaf.ID;
        //        LayoutBuilderId = (leaf as LayoutBuilderContentPlaceHolder)?.LayoutBuilderId;
        //    }

        //    public DropTarget(RuntimeContentPlaceHolder leaf, DropTargetDirective directive)
        //    {
        //        _directive = directive.ToString();
        //        PlaceHolderId = leaf.PlaceHolderId;
        //    }

        //    protected override void Render(HtmlTextWriter writer)
        //    {
        //        if (_directive == DropTargetDirective.Begin.ToString())
        //            writer.Write($"<wc-droptarget data-wc-placeholder-id='{PlaceHolderId}' data-wc-layout-builder-id='{LayoutBuilderId}' data-wc-before-page-content-id='{BeforePageContentId}'>");

        //        if (_directive == DropTargetDirective.End.ToString())
        //            writer.Write("</wc-droptarget>");               
        //    }

        //    public Guid? LayoutBuilderId { get; set; }

        //    public string PlaceHolderId { get; set; }

        //    public Guid? BeforePageContentId { get; set; }
        //}

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