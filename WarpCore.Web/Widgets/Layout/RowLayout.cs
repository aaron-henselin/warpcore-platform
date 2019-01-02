using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.DataAnnotations;

namespace WarpCore.Web.Widgets
{
    [global::Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Row (Bootstrap)", Category = "Layout")]
    public class RowLayout : Control, ILayoutControl, INamingContainer
    {
        public const string ApiId = "WC/RowLayout";

        [UserInterfaceHint][DisplayName("Number of Columns")]
        public int NumColumns { get; set; } = 1;

        [UserInterfaceIgnore]
        public Guid LayoutBuilderId { get; set; }

        public IReadOnlyCollection<PageCompositionElement> GetAutoIncludedElementsForPlaceHolder(string placeHolderId)
        {
            return new List<PageCompositionElement>();
        }

        public IReadOnlyCollection<string> InitializeLayout()
        {
            List<string> generatedPlaceHolders = new List<string>();

            var width = 12 / Math.Max(1,NumColumns);
            var row = new Panel { CssClass = "row", ID="Row" };
            for (var i = 0; i < NumColumns; i++)
            {
                var p = new Panel
                {
                    CssClass = "col-md-" + width,
                    ID=$"Column{i}"
                };
                p.Controls.Add(new LayoutBuilderContentPlaceHolder { ID = i.ToString(),LayoutBuilderId = LayoutBuilderId });
                row.Controls.Add(p);

                generatedPlaceHolders.Add(i.ToString());
            }

            this.Controls.Add(row);
            return generatedPlaceHolders;
        }


    }

    public class LayoutBuilderContentPlaceHolder : ContentPlaceHolder
    {
        public Guid LayoutBuilderId { get; set; }
    }

    
}