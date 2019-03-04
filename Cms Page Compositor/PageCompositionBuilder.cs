using System;
using System.Collections.Generic;
using System.Linq;
using Cms.Layout;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    public class PageCompositionBuilder
    {
        private readonly CmsPageContentActivator _cmsPageContentActivator;

        public PageCompositionBuilder() : this(Dependency.Resolve<CmsPageContentActivator>())
        {
            
        }

        public PageCompositionBuilder(CmsPageContentActivator cmsPageContentActivator)
        {
            _cmsPageContentActivator = cmsPageContentActivator;
        }

        public Page.Elements.PageComposition CreatePageComposition(CmsPage cmsPage)
        {
            var pageBuilder = new PageComposerTools(_cmsPageContentActivator);

            var page = new Page.Elements.PageComposition();

            page.RootElement = new UndefinedLayoutPageCompositionElement();
            if (cmsPage.LayoutId != Guid.Empty)
            {

                var layoutRepository = new LayoutRepository();
                var layoutToApply = layoutRepository.GetById(cmsPage.LayoutId);

                pageBuilder.AddLayoutContent(page, GetLayoutStructure(layoutToApply));
            }

            var pageSpecificContent = cmsPage.PageContent;

            var d = page.GetPartialPageRenderingByLayoutBuilderId();

            foreach (var contentItem in pageSpecificContent)
            {
                var placementLayoutBuilderId = contentItem.PlacementLayoutBuilderId ?? SpecialRenderingFragmentContentIds.PageRoot;
                var root = d[placementLayoutBuilderId];

                var presentationElement = contentItem.ToPresentationElement();
                pageBuilder.AddAdHocContent(presentationElement, root);
            }

            return page;
        }

        public static PageLayout GetLayoutStructure(Layout layout)
        {
            var layoutRepo = new LayoutRepository();

            PageLayout ln = null;
            if (layout.ParentLayoutId != null)
            {
                var parentLayout = layoutRepo.GetById(layout.ParentLayoutId.Value);
                ln = GetLayoutStructure(parentLayout);
            }

            return new PageLayout
            {
                AllContent = layout.PageContent.Select(x => PresentationElementHelpers.ToPresentationElement(x)).ToList(),
                MasterPagePath = layout.MasterPagePath,
                Name = layout.Name,
                ParentLayout = ln
            };
        }


        private class PageComposerTools
        {
            private readonly CmsPageContentActivator _contentActivator;

            public PageComposerTools() : this(Dependency.Resolve<CmsPageContentActivator>())
            {

            }

            public PageComposerTools(CmsPageContentActivator contentActivator)
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
                    AddContent(activatedWidget, mergedContent, isFromLayout);

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
                AddContent(page, new[] { contentToActivate }, false);
            }

            public void AddLayoutContent(IReadOnlyCollection<PageContent> contentToActivate, PageCompositionElement parentCompositionElement)
            {
                AddContent(parentCompositionElement, contentToActivate, true);
            }

            public void AddLayoutContent(Page.Elements.PageComposition page, PageLayout layoutToApply)
            {
                if (layoutToApply.ParentLayout != null)
                    AddLayoutContent(page, layoutToApply.ParentLayout);

                if (!string.IsNullOrWhiteSpace(layoutToApply.MasterPagePath))
                {
                    page.RootElement = _contentActivator.ActivateRootLayout(layoutToApply.MasterPagePath);
                }

                AddLayoutContent(layoutToApply.AllContent, page.RootElement);
            }




        }

    }
}