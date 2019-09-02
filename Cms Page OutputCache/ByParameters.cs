using System.Linq;

namespace Modules.Cms.Features.Presentation.Cache
{
    public class ByParameters : ICmsPageContentCacheKeyFactory
    {
        public string GetCacheKey(CacheKeyParts content)
        {
            //todo now: serialize this instead.
            var param = string.Join(",", content.Parameters.Keys) + "|"+string.Join(",", content.Parameters.Values);
            var parametersKey = param;
            var widgetTypeCode = content.WidgetType.AssemblyQualifiedName;
            return "by-parameters-"+widgetTypeCode + "-" + parametersKey;
        }
    }

    public class ByInstance : ICmsPageContentCacheKeyFactory
    {
        public string GetCacheKey(CacheKeyParts content)
        {
            
            var widgetTypeCode = content.WidgetType.AssemblyQualifiedName;

            return "by-instance-"+widgetTypeCode + "-" + content.ContentId;
        }
    }
}
