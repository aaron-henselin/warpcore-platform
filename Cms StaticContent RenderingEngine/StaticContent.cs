using System.Collections.Generic;

namespace Cms_StaticContent_RenderingEngine
{
    public class StaticContent
    {
        public string Html { get; set; }

        public Dictionary<string,string> GlobalContent { get; set; } = new Dictionary<string, string>();
    }
}