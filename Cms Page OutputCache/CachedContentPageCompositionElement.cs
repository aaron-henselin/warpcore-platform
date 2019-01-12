using System;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.RenderingEngines.CachedContent
{
    public class CachedContentPageCompositionElement : PageCompositionElement, IHasInternalLayout
    {

        public InternalLayout InternalLayout { get; set; }
        public RenderingResult RenderingResult { get; set; }

        public InternalLayout GetInternalLayout()
        {
            return InternalLayout;
        }
    }

    public class CachedContentActivator
    {
        private readonly CmsPageContentOutputCacheProvider _cmsPageContentOutputCacheProvider;

        public CachedContentActivator() :this(Dependency.Resolve<CmsPageContentOutputCacheProvider>()) 
        {

        }

        private CachedContentActivator(CmsPageContentOutputCacheProvider cmsPageContentOutputCacheProvider)
        {
            _cmsPageContentOutputCacheProvider = cmsPageContentOutputCacheProvider;
        }

        public bool TryCreateCachedContentElement(Type toolboxItemType, PageContent pageContent, out PageCompositionElement element, out string cacheKey)
        {
            element = null;
            CachedPageContentOutput found = null;
            var cacheKeyParts = new CacheKeyParts { ContentId = pageContent.Id, Parameters = pageContent.Parameters, WidgetType = toolboxItemType };
            var isCacheable = _cmsPageContentOutputCacheProvider.IsCacheable(toolboxItemType);
            cacheKey = null;
            if (isCacheable)
                cacheKey = _cmsPageContentOutputCacheProvider.GetCacheKey(cacheKeyParts);

            var canBeActivatedViaCache = isCacheable && _cmsPageContentOutputCacheProvider.TryResolveFromCache(cacheKey, out found);
            if (!canBeActivatedViaCache)
                return false;

            element = new CachedContentPageCompositionElement
            {
                InternalLayout = found.InternalLayout,
                RenderingResult = found.RenderingResult
            };

            return true;
        }
    }
}