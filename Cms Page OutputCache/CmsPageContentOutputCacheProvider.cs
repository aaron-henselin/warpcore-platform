using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.Cache
{

    public class CmsPageContentOutputCacheProvider
    {
        private readonly ICache _cache;

        public CmsPageContentOutputCacheProvider(ICache cache)
        {
            _cache = cache;
        }

        private static ConcurrentDictionary<Type,bool> IsCacheableLookup { get; } = new ConcurrentDictionary<Type, bool>();
      
        public void AddToCache(string cacheKey, CachedPageContentOutput incomingCache)
        {

            _cache.Add(cacheKey, incomingCache, DateTime.Now.AddMinutes(5));
        }

        public bool IsCacheable(Type type)
        {
            if (IsCacheableLookup.ContainsKey(type))
                return IsCacheableLookup[type];

            var genInterface = typeof(ISupportsCache<>);
            var cacheInterface =
                type.GetInterfaces()
                    .Where(x => x.IsGenericType)
                    .SingleOrDefault(x => x.GetGenericTypeDefinition() == genInterface);

            IsCacheableLookup.TryAdd(type, cacheInterface != null);
            return IsCacheableLookup[type];
        }

        public string GetCacheKey(CacheKeyParts parts)
        {
            var genInterface = typeof(ISupportsCache<>);
            var cacheInterface =
                parts.WidgetType.GetInterfaces()
                    .Where(x => x.IsGenericType)
                    .Single(x => x.GetGenericTypeDefinition() == genInterface);

            var cacheDirector = cacheInterface.GetGenericArguments().Single();
            var cacheKeyFactory = (ICmsPageContentCacheKeyFactory)Dependency.Resolve(cacheDirector);
            return cacheKeyFactory.GetCacheKey(parts);
        }

        public bool TryResolveFromCache(string cachekey, out CachedPageContentOutput element)
        {
            element = null;

            var cacheObject = _cache.Get<CachedPageContentOutput>(cachekey);
            if (cacheObject == null)
                return false;

            element = (CachedPageContentOutput)cacheObject;
            return true;
        }


    }

}