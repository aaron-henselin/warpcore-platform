using System;
using System.Linq;
using Cms.Layout;
using Cms_PageDesigner_Context;
using Modules.Cms.Features.Presentation.Page.Elements;
using Modules.Cms.Features.Presentation.PageComposition;
using Platform_WebPipeline;
using WarpCore.Platform.Kernel;

namespace WarpCore.Cms
{
    public class PageCompositionBuilder
    {
        private CmsPageContentActivator _cmsPageContentActivator;

        public PageCompositionBuilder() : this(Dependency.Resolve<CmsPageContentActivator>())
        {
            
        }

        public PageCompositionBuilder(CmsPageContentActivator cmsPageContentActivator)
        {
            _cmsPageContentActivator = cmsPageContentActivator;
        }

        public PageComposition CreatePageComposition(CmsPage cmsPage, PageRenderMode pageRenderMode)
        {
            var pageBuilder = new PageComposer(_cmsPageContentActivator);

            var page = new PageComposition();

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



    }

    public static class PresentationElementHelpers
    {
        public static PageContent ToPresentationElement(this CmsPageContent content)
        {
            return new PageContent
            {
                Id = content.Id,
                AllContent = content.AllContent.Select(ToPresentationElement).ToList(),
                Order = content.Order,
                Parameters = content.Parameters,
                PlacementContentPlaceHolderId = content.PlacementContentPlaceHolderId,
                PlacementLayoutBuilderId = content.PlacementLayoutBuilderId,
                WidgetTypeCode = content.WidgetTypeCode,
            };
        }
    }
}