namespace Modules.Cms.Features.Presentation.Cache
{
    public interface ICmsPageContentCacheKeyFactory
    {
        string GetCacheKey(CacheKeyParts cacheKeyParts);
    }
}