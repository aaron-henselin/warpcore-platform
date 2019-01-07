using System;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Features.Presentation.Cache
{
    [Serializable]
    public class CachedPageContentOutput
    {
        public InternalLayout InternalLayout { get; set; }
        public RenderingResult RenderingResult { get; set; }
    }

}