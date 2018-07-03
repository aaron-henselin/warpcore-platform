using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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