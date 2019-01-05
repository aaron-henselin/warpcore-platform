using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public interface IFragmentRenderer
    {
        RenderingFragmentCollection Execute(PageCompositionElement pp);
    }

}