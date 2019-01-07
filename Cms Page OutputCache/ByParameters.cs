using System.Web.Script.Serialization;

namespace Modules.Cms.Features.Presentation.Cache
{
    public class ByParameters : ICmsPageContentCacheKeyFactory
    {
        public string GetCacheKey(CacheKeyParts content)
        {
            var parametersKey = new JavaScriptSerializer().Serialize(content.Parameters);
            var widgetTypeCode = content.WidgetType.AssemblyQualifiedName;

            return widgetTypeCode + "-" + parametersKey;
        }
    }
}
