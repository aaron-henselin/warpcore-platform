using System.Linq;
using System.Web;
using System.Web.Mvc;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace WarpCore.Web.RenderingEngines.Mvc
{
    public class ControllerPageCompositionElement : PageCompositionElement, IHasInternalLayout
    {
        public IController Controller { get; }

        public ControllerPageCompositionElement(IController controller)
        {
            Controller = controller;

        }

        public InternalLayout GetInternalLayout()
        {
            return (Controller as IHasInternalLayout)?.GetInternalLayout() ?? InternalLayout.Empty;
        }
    }
}