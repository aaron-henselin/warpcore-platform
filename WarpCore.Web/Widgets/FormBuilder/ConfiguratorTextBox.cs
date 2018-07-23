using System;
using System.Web.UI.WebControls;
using Cms.Toolbox;

namespace DemoSite
{
    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorTextBox",FriendlyName = "TextBox")]
    public class ConfiguratorTextBox : PlaceHolder
    {
        private TextBox _tbx = new TextBox { AutoPostBack = true,CssClass = "form-control"};

        [Setting]
        public string PropertyName { get; set; }

        [Setting]
        public string DisplayName { get; set; }

        public string Value
        {
            get { return _tbx.Text; }
            set { _tbx.Text = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;

            this.Controls.Add(new Label { Text = DisplayName });
            _tbx.ID = PropertyName + "_TextBox";
            this.Controls.Add(_tbx);
        }


    }
}