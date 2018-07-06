using System;
using System.Web.UI.WebControls;

namespace WarpCore.Web.Widgets
{
    [IncludeInToolbox(Name = "WC/RowLayout")]
    public class RowLayout : LayoutControl
    {
        public int NumColumns { get; set; }

        public override void InitializeLayout()
        {
            var width = 12 / NumColumns;
            var row = new Panel { CssClass = "container row" };
            for (var i = 0; i < NumColumns; i++)
            {
                var p = new Panel
                {
                    CssClass = "col-md-" + width
                };
                p.Controls.Add(new LayoutBuilderContentPlaceHolder { ID = i.ToString(),LayoutBuilderId = LayoutBuilderId });
                row.Controls.Add(p);
            }

            this.Controls.Add(row);
        }
    }

    public class LayoutBuilderContentPlaceHolder : ContentPlaceHolder
    {
        public Guid LayoutBuilderId { get; set; }
    }
}