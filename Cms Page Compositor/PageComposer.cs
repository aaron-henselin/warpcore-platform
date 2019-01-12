using System;
using System.Collections.Generic;
using System.Linq;
using Cms.Layout;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    public class PageComposer
    {
        private readonly CmsPageContentActivator _contentActivator;

        public PageComposer(): this(Dependency.Resolve<CmsPageContentActivator>())
        {
           
        }

        public PageComposer(CmsPageContentActivator contentActivator)
        {
            _contentActivator = contentActivator;
        }

        private void AddContent(PageCompositionElement parentCompositionElement,
            IReadOnlyCollection<PageContent> contents, bool isFromLayout)
        {
            foreach (var content in contents)
            {
                var placementPlaceHolder = FindPlacementLocation(parentCompositionElement, content);
                if (placementPlaceHolder == null)
                    continue;

                AddContent(placementPlaceHolder, content, isFromLayout);
            }
        }



        private void AddContent(RenderingsPlaceHolder placementPlaceHolder, PageContent content,
            bool isFromLayout)
        {
            var activatedWidget = _contentActivator.ActivateCmsPageContent(content);
            activatedWidget.IsFromLayout = isFromLayout;
            
            var internalLayout = (activatedWidget as IHasInternalLayout)?.GetInternalLayout() ?? InternalLayout.Empty;

            foreach (var placeholderId in internalLayout.PlaceHolderIds)
                activatedWidget.PlaceHolders.Add(placeholderId, new RenderingsPlaceHolder(placeholderId));
            

            var mergedContent = new List<PageContent>();
            var placedByUser = content.AllContent;
            var placedByDefault = internalLayout.DefaultContent;

            mergedContent.AddRange(placedByDefault);
            mergedContent.AddRange(placedByUser);

            if (mergedContent.Any())
                AddContent(activatedWidget,mergedContent, isFromLayout);

            placementPlaceHolder.Renderings.Add(activatedWidget);
        }


        private static RenderingsPlaceHolder FindPlacementLocation(PageCompositionElement searchContext, PageContent content)
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

        public void AddLayoutContent(PageContent contentToActivate, RenderingsPlaceHolder page)
        {
            AddContent(page, contentToActivate, true);
        }

        public void AddAdHocContent(PageContent contentToActivate, PageCompositionElement page)
        {
            AddContent(page,new[]{contentToActivate},false);
        }

        public void AddLayoutContent(IReadOnlyCollection<PageContent> contentToActivate, PageCompositionElement parentCompositionElement)
        {
            AddContent(parentCompositionElement, contentToActivate, true);
        }

        public void AddLayoutContent(Page.Elements.PageComposition page, PageLayout layoutToApply)
        {
            if (layoutToApply.ParentLayout != null)
                AddLayoutContent(page,layoutToApply.ParentLayout);

            if (!string.IsNullOrWhiteSpace(layoutToApply.MasterPagePath))
            {
                page.RootElement =_contentActivator.ActivateRootLayout(layoutToApply.MasterPagePath);
            }

            AddLayoutContent(layoutToApply.AllContent,page.RootElement);
            
            //var structure = _layoutRepository.GetLayoutStructure(layoutToApply);
            //var lns = FlattenLayoutTree(structure);

            //if (lns.Any())
            //{
            //    var mpFile = layoutToApply.MasterPagePath = lns.First().Layout.MasterPagePath;
            //    page.RootElement = _contentActivator.ActivateRootLayout(mpFile);
            //}
            
            ////todo: nested layouts.
            //var root = localPage.GetRootControl();
            //foreach (var ln in layoutToApply.NestedLayouts)
            //    AddLayoutContent(ln.PageContent, page.RootElement);
        }




    }
}