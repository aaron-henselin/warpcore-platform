using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.FormBuilder.Configurators
{
    [IncludeInToolbox(WidgetUid = ApiId, FriendlyName = "Hidden", Category = "Form Controls")]
    public class ConfiguratorHiddenField : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        public const string ApiId = "warpcore-formcontrol-hiddenfield";

        private HiddenField _hiddenField = new HiddenField {  };

        [FormControlPropertiesDataSource]
        [UserInterfaceHint(Editor = Editor.OptionList)]
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
            Behaviors.AddRange(settingProperty.Behaviors.Select(x => x.AssemblyQualifiedName).ToList());

        }


        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
           
        }

        [UserInterfaceHint(Editor = Editor.Hidden)]
        public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;

            _hiddenField.ID = PropertyName + "_HiddenField";
            this.Controls.Add(_hiddenField);
        }
    }
}