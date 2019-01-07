using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition.Cache
{
    public interface ISupportsCache<TCmsPageContentCacheKeyFactory> where TCmsPageContentCacheKeyFactory : ICmsPageContentCacheKeyFactory
    {

    }

    public class CacheKeyParts
    {
        public Type WidgetType { get; set; }
        public Guid ContentId { get; set; }
        public Dictionary<string,string> Parameters { get; set; }
    }
    

    public class CmsPageContentCacheResolver
    {
        private static ConcurrentDictionary<Type,bool> IsCacheableLookup { get; } = new ConcurrentDictionary<Type, bool>();
      
        public void AddToCache(string cacheKey, CmsPageContentCache incomingCache)
        {
            HttpRuntime.Cache.Add(cacheKey, incomingCache, new CacheDependency(new string[0]), DateTime.Now.AddMinutes(5),
                System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
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

        public bool TryResolveFromCache(string cachekey, out CmsPageContentCache element)
        {
            element = null;
            
            
            var cacheObject = HttpRuntime.Cache.Get(cachekey);
            if (cacheObject == null)
                return false;

            element = (CmsPageContentCache)cacheObject;
            return true;
        }


    }

    public interface ICmsPageContentCache
    {
        bool TryActivateCmsPageContentCache(string cacheKey, out CmsPageContentCache cache);
    }

    [Serializable]
    public class CmsPageContentCache
    {
        public InternalLayout InternalLayout { get; set; }
        public RenderingResult RenderingResult { get; set; }
    }

}