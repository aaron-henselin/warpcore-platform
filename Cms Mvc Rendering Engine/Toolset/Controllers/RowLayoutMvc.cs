using System;
using System.ComponentModel;
using System.Web.Mvc;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.DataAnnotations;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc.Toolset.Controllers
{
    [global::Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Row (Bootstrap)", Category = "Layout")]
    public class RowLayout : Controller, IHasInternalLayout
    {
        public const string ApiId = "WC/RowLayout";

        [UserInterfaceHint]
        [DisplayName("Number of Columns")]
        public int NumColumns { get; set; } = 1;

        [UserInterfaceIgnore]
        public Guid LayoutBuilderId { get; set; }

        public InternalLayout GetInternalLayout()
        {
            var internalLayout = new InternalLayout();
            for (int i = 0; i < NumColumns; i++)
                internalLayout.PlaceHolderIds.Add(i.ToString());

            return internalLayout;
        }



        public ViewResult Index()
        {

            ViewBag.Css = "col-md-"+12 / NumColumns;
            ViewBag.NumColumns = NumColumns;

            return View();
        }
    }
}