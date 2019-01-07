using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms;

namespace Modules.Cms.Features.Presentation.PageComposition.Cache
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
