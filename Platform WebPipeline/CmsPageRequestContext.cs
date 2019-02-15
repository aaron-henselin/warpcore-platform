using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Platform_WebPipeline
{
    public class CmsPageRequestContext
    {
        public SiteRoute Route { get; set; }
        public CmsPage CmsPage { get; set; }
        public PageRenderMode PageRenderMode { get; set; }


    }

    public enum PageRenderMode { Readonly, PageDesigner }

}