using WarpCore.Cms;

namespace Modules.Cms.Features.Presentation.PageComposition.Cache
{
    public interface ICmsPageContentCacheKeyFactory
    {
        string GetCacheKey(CmsPageContent content);
    }
}