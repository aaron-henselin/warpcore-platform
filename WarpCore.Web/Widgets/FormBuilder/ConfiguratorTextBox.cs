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
    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorHiddenField", FriendlyName = "Hidden", Category = "Form Controls")]
    public class ConfiguratorHiddenField : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private HiddenField _hiddenField = new HiddenField {  };

        [FormControlPropertiesDataSource]
        [Setting(SettingType = SettingType.OptionList)]
        [DisplayName("Property")]
        public string PropertyName { get; set; }


        public string Value
        {
            get { return _hiddenField.Value; }
            set { _hiddenField.Value = value; }
        }

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
           
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;

            _hiddenField.ID = PropertyName + "_HiddenField";
            this.Controls.Add(_hiddenField);
        }
    }

    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorTextBox",FriendlyName = "TextBox", Category = "Form Controls")]
    public class ConfiguratorTextBox : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private TextBox _tbx = new TextBox { CssClass = "form-control"};

        [FormControlPropertiesDataSource]
        [Setting(SettingType = SettingType.OptionList)][DisplayName("Property name")]
        public string PropertyName { get; set; }

        [Setting][DisplayName("Display name")]
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

            this.Controls.Add(new Label { Text = DisplayName,CssClass = "form-label"});
            _tbx.ID = PropertyName + "_TextBox";
            this.Controls.Add(_tbx);
        }


    }
}