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

        /// <summary>
        /// Sets which Property of the object should be configured using this textbox,
        /// when a value is submitted by an end user.
        /// </summary>
        [FormControlPropertiesDataSource(typeof(string),typeof(int),typeof(decimal))]
        [Setting(SettingType = SettingType.OptionList)][DisplayName("Property name")]
        public string PropertyName { get; set; }

        /// <summary>
        /// Sets a custom label for the textbox when presented to the end user.
        /// </summary>
        [Setting][DisplayName("Display name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Determines whether or not entry of the textbox must be supplied by an end user.
        /// </summary>
        [Setting(SettingType = SettingType.CheckBox)]
        [DisplayName("Required")]
        public bool IsRequired { get; set; }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            PropertyName = settingProperty.PropertyInfo.Name;
            DisplayName = settingProperty.DisplayName;
        }


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