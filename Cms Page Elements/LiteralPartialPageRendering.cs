using System;

namespace Modules.Cms.Features.Presentation.Page.Elements
{
    public class LiteralPageCompositionElement : PageCompositionElement
    {
        
        public LiteralPageCompositionElement()
        {
            ContentId = Guid.NewGuid();
        }

        public LiteralPageCompositionElement(string text):this()
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}