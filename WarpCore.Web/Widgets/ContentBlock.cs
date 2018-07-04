using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms;

namespace WarpCore.Web.Widgets
{
    public class LayoutControl :Control
    {
        public Guid LayoutBuilderId { get; set; }
    }

    public class Column : Panel, INamingContainer
    {
    }

    [IncludeInToolbox(Name = "WC/ColumnLayout")]
    public class RowLayout : LayoutControl
    {
        public int NumColumns { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            var width = 12 / NumColumns;

            var row = new Panel { CssClass = "row"};

            for (var i = 0; i < NumColumns; i++)
            {
                var p = new Panel
                {
                    CssClass = "col_md_" + width
                };
                p.Controls.Add(new ContentPlaceHolder { ID=i.ToString()});
                row.Controls.Add(p);
            }

            this.Controls.Add(row);
        }
    }

    [IncludeInToolbox(Name="WC/ContentBlock")]
    public class ContentBlock : Control
    {
        public MultilingualString AdHocHtml { get; set; }
    }
}