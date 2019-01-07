using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Cms.Layout;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Cache;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using MoreLinq;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    public class CacheElement : PageCompositionElement
    {
    }




    public class PageComposer
    {
        private readonly LayoutRepository _layoutRepository = new LayoutRepository();
        private readonly CmsPageContentActivator _contentActivator;

        public PageComposer(): this(Dependency.Resolve<CmsPageContentActivator>())
        {
           
        }

        public PageComposer(CmsPageContentActivator contentActivator)
        {
            _contentActivator = contentActivator;
        }

        private void AddContent(PageCompositionElement parentCompositionElement,
            IReadOnlyCollection<CmsPageContent> contents, bool isFromLayout)
        {
            foreach (var content in contents)
            {
                var placementPlaceHolder = FindPlacementLocation(parentCompositionElement, content);
                if (placementPlaceHolder == null)
                    continue;

                AddContent(placementPlaceHolder, content, isFromLayout);
            }
        }

        private void AddContent(RenderingsPlaceHolder placementPlaceHolder, CmsPageContent content,
            bool isFromLayout)
        {
            var activatedWidget = _contentActivator.ActivateCmsPageContent(content);
            activatedWidget.IsFromLayout = isFromLayout;
            
            var internalLayout = (activatedWidget as IHasInternalLayout)?.GetInternalLayout() ?? InternalLayout.Empty;

            foreach (var placeholderId in internalLayout.PlaceHolderIds)
                activatedWidget.PlaceHolders.Add(placeholderId, new RenderingsPlaceHolder(placeholderId));
            

            var mergedContent = new List<CmsPageContent>();
            var placedByUser = content.AllContent;
            var placedByDefault = internalLayout.DefaultContent;

            mergedContent.AddRange(placedByDefault);
            mergedContent.AddRange(placedByUser);

            if (mergedContent.Any())
                AddContent(activatedWidget,mergedContent, isFromLayout);

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

        public void AddLayoutContent(CmsPageContent contentToActivate, RenderingsPlaceHolder page)
        {
            AddContent(page, contentToActivate, true);
        }

        public void AddAdHocContent(CmsPageContent contentToActivate, PageCompositionElement page)
        {
            AddContent(page,new[]{contentToActivate},false);
        }

        public void AddLayoutContent(IReadOnlyCollection<CmsPageContent> contentToActivate, PageCompositionElement parentCompositionElement)
        {
            AddContent(parentCompositionElement, contentToActivate, true);
        }

        public void AddLayoutContent(Elements.PageComposition page, Layout layoutToApply)
        {
           
            
            if (!string.IsNullOrWhiteSpace(layoutToApply.MasterPagePath))
                page.RootElement = _contentActivator.ActivateLayoutByExtension(layoutToApply.MasterPagePath);
            

            var structure = _layoutRepository.GetLayoutStructure(layoutToApply);
            var lns = FlattenLayoutTree(structure);

            if (lns.Any())
            {
                var mpFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
                page.RootElement = _contentActivator.ActivateLayoutByExtension(mpFile);
            }
            

            //var root = localPage.GetRootControl();
            foreach (var ln in lns)
                AddLayoutContent(ln.Layout.PageContent, page.RootElement);
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