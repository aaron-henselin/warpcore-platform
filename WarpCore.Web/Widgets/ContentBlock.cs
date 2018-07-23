using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms;

namespace WarpCore.Web.Widgets
{
    [IncludeInToolbox(WidgetUid="WC/ContentBlock", FriendlyName = "Content Block", Category = "Content")]
    public class ContentBlock : Control
    {
        [Setting]
        public string AdHocHtml { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            DataBind();
        }

        public override void DataBind()
        {
            base.DataBind();

            this.Controls.Clear();
            this.Controls.Add(new Literal { Text = AdHocHtml });
        }
    }
}