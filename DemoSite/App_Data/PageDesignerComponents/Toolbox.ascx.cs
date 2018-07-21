using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using WarpCore.Cms.Toolbox;

namespace DemoSite
{
    public class ToolboxItemViewModel
    {
        public string WidgetTypeCode { get; set; }

        public string FriendlyName { get; set; }
    }

    public partial class Toolbox : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            PopulateToolboxSidebar();

            DataBind();
        }

        private void PopulateToolboxSidebar()
        {
            var manager = new ToolboxManager();
            var allWidgets = manager.Find();
            ToolboxItemRepeater.DataSource = allWidgets.Select(x => new ToolboxItemViewModel
            {
                FriendlyName = x.Name,
                WidgetTypeCode = x.Name
            }).ToList();

            //foreach (var widget in allWidgets)
            //{
            //    var div = new HtmlGenericControl("div");
            //    div.Attributes["class"] = "toolbox-item wc-layout-handle";
            //    div.Attributes["data-wc-toolbox-item-name"] = widget.Name;

            //    div.InnerText = widget.Name;
            //    toolboxUl.Controls.Add(div);
            //}

            //toolboxUl.Controls.Add(div);
        }
    }
}