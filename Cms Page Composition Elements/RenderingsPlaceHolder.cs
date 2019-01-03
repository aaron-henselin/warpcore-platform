using System.Collections.Generic;

namespace Modules.Cms.Features.Presentation.PageComposition.Elements
{
    public class RenderingsPlaceHolder
    {
        public RenderingsPlaceHolder(string Id)
        {
            this.Id = Id;
        }

        public string Id { get; }

        public List<PageCompositionElement> Renderings { get; set; } = new List<PageCompositionElement>();
    }
}