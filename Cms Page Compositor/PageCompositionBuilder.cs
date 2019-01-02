using System;
using System.Collections.Generic;
using System.Linq;
using Cms.Layout;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    public class PageCompositionBuilder
    {
        //private readonly CmsPageRequestContext _context;
        private readonly LayoutRepository layoutRepository = new LayoutRepository();
        private CmsPageContentActivator _contentActivator;

        public PageCompositionBuilder(): this(Dependency.Resolve<CmsPageContentActivator>())
        {
           
        }


        public PageCompositionBuilder(CmsPageContentActivator contentActivator)
        {
            _contentActivator = contentActivator;
        }




        private void ActivateAndPlaceContent(PageCompositionElement parentCompositionElement,
            IReadOnlyCollection<CmsPageContent> contents, bool isFromLayout)
        {
            int i = 0;
            foreach (var content in contents)
            {
                i++;

                var activatedWidget = _contentActivator.ActivateCmsPageContent(content);
                activatedWidget.IsFromLayout = isFromLayout;

                var placementPlaceHolder = FindPlacementLocation(parentCompositionElement, content);
                if (placementPlaceHolder == null)
                    continue;

                if (content.AllContent.Any())
                    ActivateAndPlaceContent(activatedWidget, content.AllContent, isFromLayout);

                placementPlaceHolder.Renderings.Add(activatedWidget);
            }
        }
    

        private static RenderingsPlaceHolder FindPlacementLocation(PageCompositionElement searchContext, CmsPageContent content)
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

        public void ActivateAndPlaceAdHocPageContent(CmsPageContent contentToActivate, PageCompositionElement page)
        {
            ActivateAndPlaceContent(page,new[]{contentToActivate},false);
        }

        public void ActivateAndPlaceLayoutContent(IReadOnlyCollection<CmsPageContent> contentToActivate, PageCompositionElement parentCompositionElement)
        {
            ActivateAndPlaceContent(parentCompositionElement, contentToActivate, true);
        }

        public void ActivateAndPlaceLayoutContent(Elements.PageComposition page, Layout layoutToApply)
        {
           
            
            if (!string.IsNullOrWhiteSpace(layoutToApply.MasterPagePath))
                page.RootElement = _contentActivator.ActivateLayoutByExtension(layoutToApply.MasterPagePath);
            

            var structure = layoutRepository.GetLayoutStructure(layoutToApply);
            var lns = FlattenLayoutTree(structure);

            if (lns.Any())
            {
                var mpFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
                page.RootElement = _contentActivator.ActivateLayoutByExtension(mpFile);
            }
            

            //var root = localPage.GetRootControl();
            foreach (var ln in lns)
                ActivateAndPlaceLayoutContent(ln.Layout.PageContent, page.RootElement);
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