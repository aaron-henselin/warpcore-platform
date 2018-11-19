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

    [IncludeInToolbox(WidgetUid = ApiId,FriendlyName = "TextBox", Category = "Form Controls")]
    public class ConfiguratorTextBox : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        public const string ApiId = "WC/ConfiguratorTextBox";

        private TextBox _tbx = new TextBox { CssClass = "form-control"};

        /// <summary>
        /// Sets which Property of the object should be configured using this textbox,
        /// when a value is submitted by an end user.
        /// </summary>
        [FormControlPropertiesDataSource(typeof(string),typeof(int),typeof(decimal))]
        [Setting(SettingType = SettingType.OptionList)][DisplayName("Property name")]
        public string PropertyName { get; set; }

        [DisplayName("TextBox Type")]
        [Setting(SettingType = SettingType.OptionList)]
        [TextBoxModesDataSource]
        public string TextBoxMode { get; set; }

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

            if (new[]{typeof(int),typeof(decimal)}.Contains(settingProperty.PropertyInfo.PropertyType))
                TextBoxMode = System.Web.UI.WebControls.TextBoxMode.Number.ToString();

            if (new[] { typeof(DateTime) }.Contains(settingProperty.PropertyInfo.PropertyType))
                TextBoxMode = System.Web.UI.WebControls.TextBoxMode.DateTime.ToString();

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

            if (!string.IsNullOrWhiteSpace(TextBoxMode))
                _tbx.TextMode = (TextBoxMode)Enum.Parse(typeof(TextBoxMode), TextBoxMode, true);

            this.Controls.Add(_tbx);
        }


        private class TextBoxModesDataSourceAttribute : Attribute, IListControlSource
        {
            public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
            {
                return Enum.GetNames(typeof(TextBoxMode)).Select(x => new ListOption { Text = x, Value = x }).ToList();
            }
        }

    }


}