using System.Linq;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;

namespace Modules.Cms.Features.Presentation.RenderingEngines.CachedContent
{
    public class CachedContentFragmentRenderer : IFragmentRenderer
    {
        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {
            var batch = new RenderingFragmentCollection();
            var allDesc = pp.GetAllDescendents().OfType<CachedContentPageCompositionElement>();
            foreach (var item in allDesc)
                batch.RenderingResults.Add(item.ContentId, item.RenderingResult);
            
            return batch;
        }
    }
}
