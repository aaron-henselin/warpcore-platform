using System.Linq;
using System.Web;
using System.Web.Mvc;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace WarpCore.Web.RenderingEngines.Mvc
{
    public class ControllerPageCompositionElement : PageCompositionElement
    {
        public IController Controller { get; }

        public ControllerPageCompositionElement(IController controller)
        {
            Controller = controller;

        }
    }
}