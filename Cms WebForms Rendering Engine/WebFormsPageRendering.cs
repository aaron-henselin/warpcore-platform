using System.Web.UI;
using System.Web.UI.WebControls;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{

    public interface IHandledByWebFormsRenderingEngine
    {
    }

    public class WebFormsPageCompositionElement : PageCompositionElement, IHandledByWebFormsRenderingEngine
    {
        private readonly Page _masterPage;

        public WebFormsPageCompositionElement(Page page)
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


        public Page GetPage()
        {
            return _masterPage;
        }

    }
}