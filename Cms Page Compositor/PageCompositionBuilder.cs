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
            foreach (var content in contents)
            {
                var placementPlaceHolder = FindPlacementLocation(parentCompositionElement, content);
                if (placementPlaceHolder == null)
                    continue;

                ActivateAndPlaceContent(placementPlaceHolder, content, isFromLayout);
            }
        }

        private void ActivateAndPlaceContent(RenderingsPlaceHolder placementPlaceHolder, CmsPageContent content,
            bool isFromLayout)
        {
            var activatedWidget = _contentActivator.ActivateCmsPageContent(content);
            activatedWidget.IsFromLayout = isFromLayout;
            
            var internalLayout = (activatedWidget as IHasInternalLayout)?.GetInternalLayout();
            foreach (var placeholderId in internalLayout.PlaceHolderIds)
                activatedWidget.PlaceHolders.Add(placeholderId, new RenderingsPlaceHolder(placeholderId));

            var mergedContent = new List<CmsPageContent>();
            var placedByUser = content.AllContent;
            var placedByDefault = internalLayout.DefaultContent;

            mergedContent.AddRange(placedByDefault);
            mergedContent.AddRange(placedByUser);

            if (mergedContent.Any())
                ActivateAndPlaceContent(activatedWidget,mergedContent, isFromLayout);

            placementPlaceHolder.Renderings.Add(activatedWidget);
        }


        private static RenderingsPlaceHolder FindPlacementLocation(PageCompositionElement searchContext, CmsPageContent content)
        {
            if (!string.IsNullOrWhiteSpace(content.PlacementContentPlaceHolderId))
            try
            {
                return searchContext.PlaceHolders[content.PlacementContentPlaceHolderId];
            }
            catch (KeyNotFoundException)
            {
                
            }
            return searchContext.PlaceHolders.FirstOrDefault().Value; //should this be ordered?
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

        public void ActivateAndPlaceLayoutContent(CmsPageContent contentToActivate, RenderingsPlaceHolder page)
        {
            ActivateAndPlaceContent(page, contentToActivate, true);
        }


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