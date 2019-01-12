using System;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Context
{



    public interface ILayoutHandle
    {
        string FriendlyName { get; set; }
        string HandleName { get; set; }
        Guid PageContentId { get; set; }
    }




    public class CmsPageRequestContext
    {
        public SiteRoute Route { get; set; }
        public CmsPage CmsPage { get; set; }
        public PageRenderMode PageRenderMode { get; set; }

        public static CmsPageRequestContext Current => Dependency.Resolve<CmsPageRequestContext>();
            
          

    }

    public enum PageRenderMode { Readonly, PageDesigner }


}
