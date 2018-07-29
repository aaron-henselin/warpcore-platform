using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using Framework;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;

namespace WarpCore.Web.Widgets.FormBuilder
{
    

    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorTextBox",FriendlyName = "TextBox", Category = "Form Controls")]
    public class ConfiguratorTextBox : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private TextBox _tbx = new TextBox { CssClass = "form-control"};

        [Setting]
        public string PropertyName { get; set; }

        [Setting]
        public string DisplayName { get; set; }

        public string Text
        {
            get { return _tbx.Text; }
            set { _tbx.Text = value; }
        }

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
            //throw new NotImplementedException();
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