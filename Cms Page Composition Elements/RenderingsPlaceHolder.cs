using System.Collections.Generic;

namespace Modules.Cms.Features.Presentation.PageComposition.Elements
{
    public class RenderingsPlaceHolder
    {
        public string Id { get; set; }

        public List<PageCompositionElement> Renderings { get; set; } = new List<PageCompositionElement>();
    }
}