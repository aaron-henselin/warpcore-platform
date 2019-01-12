using Modules.Cms.Features.Presentation.Page.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public interface IFragmentRenderer
    {
        RenderingFragmentCollection Execute(PageCompositionElement pp);
    }

}