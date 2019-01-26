using System.Web.Script.Serialization;

namespace Modules.Cms.Features.Presentation.Cache
{
    public class ByParameters : ICmsPageContentCacheKeyFactory
    {
        public string GetCacheKey(CacheKeyParts content)
        {
            var parametersKey = new JavaScriptSerializer().Serialize(content.Parameters);
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
