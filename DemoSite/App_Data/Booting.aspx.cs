using System;

namespace WarpCore.Web
{
    public partial class Booting : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (WebBootstrapper.IsBooted)
            {
                var returnUrl = Request["ReturnUrl"];
                Response.Redirect(returnUrl ?? "/");
            }
        }
    }
}