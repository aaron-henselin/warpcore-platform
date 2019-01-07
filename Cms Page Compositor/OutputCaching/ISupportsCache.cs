using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

    public interface ICmsPageContentCacheResolver
    {
        bool TryResolveFromCache(CmsPageContent pageContent, out CmsPageContentCache element);
    }

    public class CmsPageContentCacheResolver : ICmsPageContentCacheResolver
    {
        public bool TryResolveFromCache(CmsPageContent pageContent, out CmsPageContentCache element)
        {
            element = null;

            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            var toolboxItemType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);

            var genInterface = typeof(ISupportsCache<>);
            var cacheInterface =
                toolboxItemType.GetInterfaces()
                    .Where(x => x.ContainsGenericParameters)
                    .SingleOrDefault(x => x.GetGenericTypeDefinition() == genInterface);

            if (cacheInterface == null)
                return false;

            var cacheDirector = cacheInterface.GetGenericArguments().Single();
            var cacheKeyFactory = (ICmsPageContentCacheKeyFactory)Dependency.Resolve(cacheDirector);
            var cacheKey = cacheKeyFactory.GetCacheKey(pageContent);
            var cacheObject = HttpRuntime.Cache.Get(cacheKey);
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
        public Dictionary<string, RenderingsPlaceHolder> PlaceHolders { get; } = new Dictionary<string, RenderingsPlaceHolder>();
        public List<string> GlobalPlaceHolders { get; } = new List<string>();
        public List<IRenderingFragment> RenderingFragments { get; } = new List<IRenderingFragment>();
    }

}