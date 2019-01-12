using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.DataAnnotations;

namespace WarpCore.Web.Widgets
{



    [WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Row (Bootstrap)", Category = "Layout")]
    public class RowLayout2 : Control, INamingContainer, IHasInternalLayout
    {
        public const string ApiId = "WC/RowLayout_WebForms";

        [UserInterfaceHint][DisplayName("Number of Columns")]
        public int NumColumns { get; set; } = 1;

        [UserInterfaceIgnore]
        public Guid LayoutBuilderId { get; set; }

        private void EnsureLayoutInitialized()
        {
            if (this.Controls.Count > 0)
                return;

            var width = 12 / Math.Max(1, NumColumns);
            var row = new Panel { CssClass = "row", ID = "Row" };
            for (var i = 0; i < NumColumns; i++)
            {
                var p = new Panel
                {
                    CssClass = "col-md-" + width,
                    ID = $"Column{i}"
                };
                p.Controls.Add(new LayoutBuilderContentPlaceHolder { ID = i.ToString(), LayoutBuilderId = LayoutBuilderId });
                row.Controls.Add(p);

            }
            this.Controls.Add(row);
        }

        public InternalLayout GetInternalLayout()
        {
           EnsureLayoutInitialized();

            var internalLayout = new InternalLayout();
            for (var i = 0; i < NumColumns; i++)
                internalLayout.PlaceHolderIds.Add(i.ToString());

            return internalLayout;
        }




    }

    public class LayoutBuilderContentPlaceHolder : ContentPlaceHolder
    {
        public Guid LayoutBuilderId { get; set; }
    }

    
}