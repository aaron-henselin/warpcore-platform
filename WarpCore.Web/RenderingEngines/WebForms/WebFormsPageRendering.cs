using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets;

namespace WarpCore.Web
{

    public interface IHandledByWebFormsRenderingEngine
    {
    }

    public class WebFormsPageRendering : PartialPageRendering, IHandledByWebFormsRenderingEngine
    {
        private readonly Page _masterPage;

        public WebFormsPageRendering(Page masterPage)
        {
            _masterPage = masterPage;
            var placeHolders = _masterPage.GetRootControl().GetDescendantControls<ContentPlaceHolder>();
            foreach (var nativePlaceHolder in placeHolders)
                this.PlaceHolders.Add(nativePlaceHolder.ID,new RenderingsPlaceHolder {Id = nativePlaceHolder.ID});

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