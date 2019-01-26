using Modules.Cms.Features.Presentation.Page.Elements;

namespace Cms_StaticContent_RenderingEngine
{
    public class StaticContentPageElement :PageCompositionElement
    {
        public StaticContent ContentControl { get; }

        public StaticContentPageElement(StaticContent contentControl)
        {
            ContentControl = contentControl;
        }
    }
}