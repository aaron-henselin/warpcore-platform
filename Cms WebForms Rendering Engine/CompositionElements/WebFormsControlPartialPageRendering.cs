using System;
using System.Web.UI;
using Modules.Cms.Features.Presentation.Page.Elements;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{
    public class WebFormsControlPageCompositionElement : PageCompositionElement,
        IHandledByWebFormsRenderingEngine, 
        IHasInternalLayout
    {
        private readonly Control _activatedWidget;

        public WebFormsControlPageCompositionElement(Control activatedWidget)
        {
            this._activatedWidget = activatedWidget;
            this.LocalId = activatedWidget.ID;

        }

        public Type GetConfigurationType()
        {
            return _activatedWidget.GetType();
        }

        public Control GetControl()
        {
            return _activatedWidget;
        }

        public InternalLayout GetInternalLayout()
        {
            return (_activatedWidget as IHasInternalLayout)?.GetInternalLayout() ?? InternalLayout.Empty;
        }
    }
}