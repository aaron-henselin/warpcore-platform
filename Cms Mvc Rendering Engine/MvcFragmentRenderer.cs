using System;
using System.Linq;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Web.RenderingEngines.Mvc;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc
{
    public class MvcFragmentRenderer : IFragmentRenderer
    {
        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {
            var collection = new RenderingFragmentCollection();
            var allDescendents = pp.GetAllDescendents().OfType<ControllerPageCompositionElement>();
            foreach (var item in allDescendents)
            {
                throw new Exception();
            }
            return collection;
        }
    }
}