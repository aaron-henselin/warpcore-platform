using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Web.Widgets;

namespace Modules.Cms.Features.Presentation.PageComposition.Elements
{
    public class PageCompositionElement
    {
        public Guid ContentId { get; set; }
        public string LocalId { get; set; }
        public Guid LayoutBuilderId { get; set; }
        public bool IsFromLayout { get; set; }
        public string FriendlyName { get; set; }
        public Dictionary<string, RenderingsPlaceHolder> PlaceHolders { get; } = new Dictionary<string, RenderingsPlaceHolder>();
        public List<string> GlobalPlaceHolders { get; } = new List<string>();

        protected void TryActivateLayout(ILayoutControl layout)
        {
            if (layout == null)
                return;

            var generatedPlaceHolders = layout.InitializeLayout();
            foreach (var ph in generatedPlaceHolders)
            {
                this.PlaceHolders.Add(ph, new RenderingsPlaceHolder { Id = ph, Renderings = layout.GetAutoIncludedElementsForPlaceHolder(ph).ToList() });
            }

            this.LayoutBuilderId = layout.LayoutBuilderId;
        }

        public IReadOnlyCollection<PageCompositionElement> GetAllDescendents()
        {
            return GetAllDescendents(this);
        }

        private IReadOnlyCollection<PageCompositionElement> GetAllDescendents(PageCompositionElement parent)
        {
            List<PageCompositionElement> partials = new List<PageCompositionElement>();

            partials.Add(parent);

            foreach (var ph in parent.PlaceHolders.Values)
            foreach (var child in ph.Renderings)
            {
                var desc = GetAllDescendents(child);
                partials.AddRange(desc);
            }

            return partials;
        }
    }
}