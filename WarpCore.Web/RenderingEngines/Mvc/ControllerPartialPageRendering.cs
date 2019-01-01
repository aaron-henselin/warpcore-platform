using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WarpCore.Web.RenderingEngines.Mvc
{
    public class ControllerPartialPageRendering : PartialPageRendering
    {
        public IController Controller { get; }

        public ControllerPartialPageRendering(IController controller)
        {
            Controller = controller;

        }
    }
}