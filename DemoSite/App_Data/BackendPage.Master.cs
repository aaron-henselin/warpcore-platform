using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Web;

namespace DemoSite
{
    public partial class BackendPage : System.Web.UI.MasterPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageHeader.DataBind();
        }

        protected void CreateNewPageButton_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("/admin/settings");
             //var ctd = "/admin/settings?contentId=" + pageTreeItem.PageId;

        }
    }
}