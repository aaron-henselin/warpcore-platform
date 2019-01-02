namespace Modules.Cms.Features.Presentation.PageComposition.Elements
{
    public class UndefinedLayoutPageCompositionElement : PageCompositionElement
    {
        public UndefinedLayoutPageCompositionElement()
        {
            PlaceHolders.Add("Body",new RenderingsPlaceHolder {Id="Body"});
        }
    }
}