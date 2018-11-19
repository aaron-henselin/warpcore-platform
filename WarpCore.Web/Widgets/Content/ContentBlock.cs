using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;

namespace WarpCore.Web.Widgets.Content
{
    [IncludeInToolbox(WidgetUid=ApiId, FriendlyName = "Content Block", Category = "Content")]
    public class ContentBlock : Control
    {
        public const string ApiId = "warpcore-content-html";

        [Setting][DisplayName("Html")]
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