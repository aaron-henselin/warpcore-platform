using WarpCore.Cms;

namespace Platform_WebPipeline.Requests
{
    public class CmsPageRequest
    {
        public SiteRoute Route { get; set; }
        public CmsPage CmsPage { get; set; }
        public PageRenderMode PageRenderMode { get; set; }


    }

    public enum PageRenderMode { Readonly, PageDesigner }

}