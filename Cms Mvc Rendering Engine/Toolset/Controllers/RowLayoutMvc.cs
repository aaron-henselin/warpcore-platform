using System;
using System.ComponentModel;
using System.Web.Mvc;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.DataAnnotations;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc.Toolset.Controllers
{
    [WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Row (Bootstrap)", Category = "Layout")]
    public class RowLayout : Controller, IHasInternalLayout, ISupportsCache<ByParameters>
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