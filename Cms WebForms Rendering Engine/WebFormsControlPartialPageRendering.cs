using System;
using System.Linq;
using System.Web.UI;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using Modules.Cms.Features.Presentation.RenderingEngines.WebForms;
using WarpCore.Web.Widgets;

namespace WarpCore.Web
{
    public class WebFormsControlPageCompositionElement : PageCompositionElement, IHandledByWebFormsRenderingEngine
    {
        private Control activatedWidget;

        public WebFormsControlPageCompositionElement(Control activatedWidget)
        {
            this.activatedWidget = activatedWidget;
            this.LocalId = activatedWidget.ID;

            TryActivateLayout(activatedWidget as ILayoutControl);
        }


        public Control GetControl()
        {
            return activatedWidget;
        }
    }
}