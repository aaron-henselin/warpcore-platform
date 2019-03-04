using System.Linq;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Cms;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    public static class PresentationElementHelpers
    {
        public static PageContent ToPresentationElement(this CmsPageContent content)
        {
            return new PageContent
            {
                Id = content.Id,
                AllContent = content.AllContent.Select(ToPresentationElement).ToList(),
                Order = content.Order,
                Parameters = content.Parameters,
                PlacementContentPlaceHolderId = content.PlacementContentPlaceHolderId,
                PlacementLayoutBuilderId = content.PlacementLayoutBuilderId,
                WidgetTypeCode = content.WidgetTypeCode,
            };
        }
    }
}