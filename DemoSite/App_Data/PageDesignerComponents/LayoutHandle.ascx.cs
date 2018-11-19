using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Web;

namespace DemoSite
{
    public partial class LayoutHandle : System.Web.UI.UserControl, ILayoutHandle
    {
        public string HandleName { get; set; }
        public Guid PageContentId { get; set; }
        public string FriendlyName { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            DataBind();
        }
    }
}