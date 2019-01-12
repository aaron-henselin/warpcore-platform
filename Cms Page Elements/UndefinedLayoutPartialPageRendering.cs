namespace Modules.Cms.Features.Presentation.Page.Elements
{
    public class UndefinedLayoutPageCompositionElement : PageCompositionElement
    {
        public UndefinedLayoutPageCompositionElement()
        {
            PlaceHolders.Add("Body",new RenderingsPlaceHolder("Body"));
        }
    }
}