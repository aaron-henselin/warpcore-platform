using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.DataAnnotations;

namespace DemoSite.Controllers
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
            return View();
        }
    }
}