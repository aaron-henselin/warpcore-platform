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
    public partial class Demo : System.Web.UI.MasterPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            PopulateToolboxSidebar();
            PopulateConfigurationSidebar();
        }

        private void PopulateConfigurationSidebar()
        {
            //throw new NotImplementedException();
        }

        private void PopulateToolboxSidebar()
        {
            var manager = new ToolboxManager();
            var allWidgets = manager.Find();

            foreach (var widget in allWidgets)
            {
                var div = new HtmlGenericControl("div");
                div.Attributes["class"] = "toolbox-item wc-layout-handle";
                div.Attributes["data-wc-toolbox-item-name"] = widget.Name;

                div.InnerText = widget.Name;
                toolboxUl.Controls.Add(div);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}