using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = ApiId, FriendlyName = "Hidden", Category = "Form Controls")]
    public class ConfiguratorHiddenField : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        public const string ApiId = "warpcore-formcontrol-hiddenfield";

        private HiddenField _hiddenField = new HiddenField {  };

        [FormControlPropertiesDataSource]
        [Setting(SettingType = SettingType.OptionList)]
        [DisplayName("Property")]
        public string PropertyName { get; set; }

        public void SetValue(string newValue)
        {
            _hiddenField.Value = newValue;
        }

        public string GetValue()
        {
            return _hiddenField.Value;
        }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            PropertyName = settingProperty.PropertyInfo.Name;
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
}