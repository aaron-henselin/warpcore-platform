using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;

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
}