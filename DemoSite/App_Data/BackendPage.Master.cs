using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public partial class BackendPage : System.Web.UI.MasterPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PageHeader.DataBind();
        }


    }
}