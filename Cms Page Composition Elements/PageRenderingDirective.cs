using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.Cms.Features.Presentation.PageComposition.Elements
{

    public static class SpecialRenderingFragmentContentIds
    {
        public static Guid PageRoot = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        public static Guid WebFormsInterop = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
    }
    public class PageComposition
    {
        public Guid CompositionId { get; set; } = Guid.NewGuid();
        public PageCompositionElement RootElement { get; set; }
        public List<string> Scripts { get; set; }
        public List<string> Styles { get; set; }

        public Dictionary<Guid, PageCompositionElement> GetPartialPageRenderingByLayoutBuilderId()
        {
            Dictionary<Guid,PageCompositionElement> d = new Dictionary<Guid, PageCompositionElement>();
            
            d = RootElement.GetAllDescendents().Where(x => x.LayoutBuilderId != Guid.Empty).ToDictionary(x => x.LayoutBuilderId);
            d.Add(SpecialRenderingFragmentContentIds.PageRoot, RootElement);

            return d;
        }


       
    }
}