using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public interface IBatchingRenderEngine
    {
        RenderingFragmentCollection Execute(PageCompositionElement pp);
    }

}