using System;
using System.Collections.Generic;

namespace Modules.Cms.Features.Presentation.Cache
{
    public class CacheKeyParts
    {
        public Type WidgetType { get; set; }
        public Guid ContentId { get; set; }
        public Dictionary<string,string> Parameters { get; set; }
    }

}