using System.Collections.Generic;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Cache;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    public class CacheElement : PageCompositionElement, IHasInternalLayout
    {
        public CacheElement(CmsPageContentCache found)
        {
            InternalLayout = found.InternalLayout;
            RenderingResult = found.RenderingResult;

        }

        public InternalLayout InternalLayout { get; set; }
        public RenderingResult RenderingResult { get; set; }

        public InternalLayout GetInternalLayout()
        {
            return InternalLayout;
        }
    }
}