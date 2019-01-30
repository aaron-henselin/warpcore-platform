using System;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;

namespace DemoSite.UserControls
{
    [ToolboxItem(AscxPath = "/UserControls/CustomNavigation.ascx",
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