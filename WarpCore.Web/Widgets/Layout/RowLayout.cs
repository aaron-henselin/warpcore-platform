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
            var row = new Panel { CssClass = "row" };
            for (var i = 0; i < NumColumns; i++)
            {
                var p = new Panel
                {
                    CssClass = "col_md_" + width
                };
                p.Controls.Add(new ContentPlaceHolder { ID = i.ToString() });
                row.Controls.Add(p);
            }

            this.Controls.Add(row);
        }
    }
}