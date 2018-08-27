using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;

namespace DemoSite.UserControls
{
    [IncludeInToolbox(AscxPath = "/UserControls/CustomNavigation.ascx",
        FriendlyName = "Navigation Bar",
        Category = "Navigation",
        WidgetUid = "Client-CustomNavigation")]
    public partial class CustomNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}