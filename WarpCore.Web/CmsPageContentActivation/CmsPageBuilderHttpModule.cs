using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
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

        //private void DeterminePlacementOrder(IReadOnlyCollection<CmsPageContent> contents)
        //{
        //    var waitingToPlace =
        //        _context.CmsPage.PageContent.Select((x, y) => new
        //            {
        //                ActivatedControl = CmsPageContentActivator.ActivateControl(x),
        //                ContentInfo = x,
        //                OriginalOrder = x.Order
        //            }).ToList();


        //    var layoutsPending = waitingToPlace.Where(x => x.ActivatedControl is LayoutControl)
        //        .Select(x => new
        //        {
        //            ActivatedLayout = (LayoutControl)x.ActivatedControl,
        //            ContentInfo = x.ContentInfo,
        //             x.OriginalOrder
        //        })
        //        .ToList();

        //    var layoutBuildersToBeCreated = layoutsPending.ToDictionary(x => x.ActivatedLayout.LayoutBuilderId,x => x);

        //    var layoutsRemainingToBePlaced = layoutsPending.ToList();

        //    List<LayoutPlacement> placements = new List<LayoutPlacement>();
        //    while (layoutsRemainingToBePlaced.Any())
        //    {
        //        int resolvedInThisRound = 0;
        //        foreach (var layout in layoutsRemainingToBePlaced)
        //        {
        //            if (layout.ContentInfo.PlacementLayoutBuilderId == null)
        //            {
        //                layoutsRemainingToBePlaced.Remove(layout);
        //                placements.Add(new LayoutPlacement
        //                {
        //                    ActivatedControl = layout.ActivatedLayout
        //                });
        //            }
        //            else
        //            {
        //                placements.
        //            }

                  
                   
        //        }

        //        if (resolvedInThisRound == 0)
        //            break;
        //    }


        //    foreach (var layout in layoutsPending)
        //        {
        //            layout.ContentInfo.PlacementLayoutBuilderId

        //        }

            

           
        //    //var first = waitingToPlace.Where(x => x.ActivatedControl is LayoutControl && x.ContentInfo.LayoutBuilderId == null);
        //    //var second = waitingToPlace.Where(x => x.ActivatedControl is LayoutControl && x.ContentInfo.LayoutBuilderId != null);


        //    //    .OrderByDescending(x => x.ActivatedControl is LayoutControl)
        //    //    .ThenByDescending(x => x.ContentInfo.LayoutBuilderId == null);


        //}


        public void PopulateContentPlaceholders(Page page, IReadOnlyCollection<CmsPageContent> contents, ViewMode vm)
        {            
            foreach (var content in _context.CmsPage.PageContent)
            {
                var activatedWidget = CmsPageContentActivator.ActivateControl(content);

                Control searchContext = page.Master;
                if (content.PlacementLayoutBuilderId != null)
                {
                    var subLayout = searchContext.GetDescendantControls<LayoutControl>()
                        .SingleOrDefault(x => x.LayoutBuilderId == content.PlacementLayoutBuilderId);

                    if (subLayout != null)
                        searchContext = subLayout;
                }

                var ph = searchContext.GetDescendantControls<ContentPlaceHolder>().SingleOrDefault(x => x.ID == content.PlacementContentPlaceHolderId);
                if (ph == null)
                    ph = searchContext.GetDescendantControls<ContentPlaceHolder>().First();

                if (vm == ViewMode.Edit)
                {
                    ph.Controls.Add(new DropTarget { BeforePageContentId = content.Id, PlaceHolderId = ph.ID});
                    var toolboxItem = new ToolboxManager().GetToolboxItemByCode(content.WidgetTypeCode);
                    var handle = new LayoutHandle { PageContentId = content.Id, HandleName = toolboxItem.Name };
                    ph.Controls.Add(handle);
                }

                ph.Controls.Add(activatedWidget);

                if (content.SubContent.Any())
                    PopulateContentPlaceholders(page, content.SubContent,vm);
                
            }
        }

        private class DropTarget : PlaceHolder
        {
            protected override void OnInit(EventArgs e)
            {
                base.OnInit(e);

                var p = new Panel();
                p.Attributes["data-wc-role"] = "droptarget";
                p.Attributes["data-wc-placeholder-id"] = PlaceHolderId.ToString();
                p.Attributes["data-wc-before-page-content-id"] = BeforePageContentId.ToString();
                p.Attributes["class"] = "wc-droptarget";
               
                this.Controls.Add(p);
            }

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

        public void AddAdHocPageContent(Page page)
        {
            if (_context.CmsPage == null)
                return;

            var leaves = IdentifyLeaves(page);
            PopulateContentPlaceholders(page,_context.CmsPage.PageContent,_context.ViewMode);
            foreach (var leaf in leaves)
            {
                leaf.Controls.Add(new DropTarget{PlaceHolderId = leaf.ID});
            }

            if (_context.ViewMode == ViewMode.Edit)
            {
                if (page.Form != null)
                {
                    page.Form.Controls.Add(new ProxiedScriptManager());
                    ScriptManagerExtensions.RegisterScriptToRunEachFullOrPartialPostback(page, "warpcore.page.edit();");
                }
            }
        }



        public void ApplyLayout(Page localPage)
        {
            var layoutToApply = layoutRepository.GetById(_context.CmsPage.LayoutId);
            localPage.MasterPageFile = layoutToApply.MasterPagePath;
            var structure=layoutRepository.GetLayoutStructure(layoutToApply);

            var lns = FlattenLayoutTree(structure);
            foreach (var ln in lns)
            {
                PopulateContentPlaceholders(localPage,ln.Layout.PageContent,ViewMode.Default);
            }
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

            var p = new Panel();
            p.Attributes["data-wc-role"] = "layout-handle";
            p.Attributes["data-wc-page-content-id"] = PageContentId.ToString();
            p.Attributes["class"] = "wc-layout-handle";
            p.Controls.Add(new Label{Text=HandleName});
            this.Controls.Add(p);
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
       
                RegisterPagePreRenderEventHandler();
        }

        private static void RegisterPagePreRenderEventHandler()
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
                    pageBuilder.ApplyLayout(localPage);
                    pageBuilder.AddAdHocPageContent(localPage);
                };
            }
        }


        void IHttpModule.Dispose()
        {
        }
    }
}