using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.Content
{
    [CompositeConfiguratorType]
    public class ComplexDemo
    {
        public string Test1 { get; set; }
        public string Test2 { get; set; }
    }

    [IncludeInToolbox(WidgetUid=ApiId, FriendlyName = "Content Block", Category = "Content")]
    public class ContentBlock : Control
    {
        public const string ApiId = "warpcore-content-html";

        [UserInterfaceHint(Editor = Editor.RichText)][DisplayName("Html")]
        public string AdHocHtml { get; set; }

        //public ComplexDemo ComplexDemo { get; set; }

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