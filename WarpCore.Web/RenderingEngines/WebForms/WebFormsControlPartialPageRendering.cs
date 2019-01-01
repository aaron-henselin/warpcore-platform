using System;
using System.Web.UI;
using WarpCore.Web.Widgets;

namespace WarpCore.Web
{
    public class WebFormsControlPartialPageRendering : PartialPageRendering, IHandledByWebFormsRenderingEngine
    {
        private Control activatedWidget;

        public WebFormsControlPartialPageRendering(Control activatedWidget)
        {
            this.activatedWidget = activatedWidget;

            this.LocalId = activatedWidget.ID;
            var layout = this.activatedWidget as ILayoutControl;
            if (layout != null)
                this.LayoutBuilderId = layout.LayoutBuilderId;
        }


        public Control GetControl()
        {
            return activatedWidget;
        }
    }
}