using System;
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
            var layout = this.activatedWidget as ILayoutControl;
            if (layout != null)
            {
                layout.InitializeLayout();
                this.LayoutBuilderId = layout.LayoutBuilderId;
            }

        }


        public Control GetControl()
        {
            return activatedWidget;
        }
    }
}