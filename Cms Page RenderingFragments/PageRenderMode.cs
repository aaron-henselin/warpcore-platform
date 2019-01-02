using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public enum FragmentRenderMode
    {
        Readonly, PageDesigner
    }


    public interface IBatchingRenderEngine
    {
        RenderingFragmentCollection Execute(PageCompositionElement pp);
    }

}