using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorTextBox",FriendlyName = "TextBox", Category = "Form Controls")]
    public class ConfiguratorTextBox : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private TextBox _tbx = new TextBox { CssClass = "form-control"};

        [FormControlPropertiesDataSource(typeof(string),typeof(int),typeof(decimal))]
        [Setting(SettingType = SettingType.OptionList)][DisplayName("Property name")]
        public string PropertyName { get; set; }

        [Setting][DisplayName("Display name")]
        public string DisplayName { get; set; }

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
            //throw new NotImplementedException();
        }

        public void SetValue(string newValue)
        {
            _tbx.Text = newValue;
        }

        public string GetValue()
        {
            return _tbx.Text;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;

            this.Controls.Add(new Label { Text = DisplayName,CssClass = "form-label"});
            _tbx.ID = PropertyName + "_TextBox";
            this.Controls.Add(_tbx);
        }


    }
}