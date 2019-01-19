using System.Web.UI.WebControls;
using Modules.Cms.Features.Presentation.Page.Elements;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{
    public class WebFormsPageCompositionElement : PageCompositionElement, 
        IHandledByWebFormsRenderingEngine
    {
        private readonly System.Web.UI.Page _masterPage;

        public WebFormsPageCompositionElement(System.Web.UI.Page page)
        {
            _masterPage = page;

            var placeHolders = _masterPage.GetRootControl().GetDescendantControls<ContentPlaceHolder>();
            foreach (var nativePlaceHolder in placeHolders)
                this.PlaceHolders.Add(nativePlaceHolder.ID, new RenderingsPlaceHolder(nativePlaceHolder.ID));

            this.GlobalPlaceHolders.Add(GlobalLayoutPlaceHolderIds.Head);
            this.GlobalPlaceHolders.Add(GlobalLayoutPlaceHolderIds.Scripts);
            this.GlobalPlaceHolders.Add(GlobalLayoutPlaceHolderIds.InternalStateTracking);

            IsFromLayout = true;
        }


        public System.Web.UI.Page GetPage()
        {
            return _masterPage;
        }

    }
}