using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.FormBuilder.Configurators
{
    [IncludeInToolbox(WidgetUid = ApiId, FriendlyName = "Composite Configurator", Category = "Form Controls")]
    public class CompositeConfigurator : PlaceHolder, IConfiguratorControl
    {
        public const string ApiId = "warpcore-formcontrol-compositeconfigurator";
        
        
        [CompositeOnlyPropertiesDataSource]
        public string PropertyName { get; set; }

        //[UserInterfaceBehavior()]
        public string DisplayName { get; set; }

        private class ConfigurationWrapper
        {
            public string TypeName { get; set; }
            public Dictionary<string,string> SerializedConfiguration { get; set; }
        }

        private PlaceHolder _surface;
        private CompositeFormReadWriter _readWriter;

        public void InitializeEditingContext(ConfiguratorBuildArguments buildArguments)
        {
            _surface = new PlaceHolder();
            _surface.Controls.Add(new Label{Text=DisplayName, CssClass = "form-label" });
            _surface.Controls.Add(new RuntimeContentPlaceHolder{ PlaceHolderId = "FormBody" });
            this.Controls.Add(_surface);

            var configType = ConfiguratorEditingContextHelper.GetClrType(buildArguments.ParentEditingContext);

            _readWriter = new CompositeFormReadWriter(configType, _surface);

            var cmsForm = ConfiguratorFormBuilder.GenerateDefaultForm(configType);
            CmsPageLayoutEngine.ActivateAndPlaceContent(_surface, cmsForm.DesignedContent);
            CmsFormReadWriter.InitializeEditing(_surface, buildArguments);
            //CmsFormReadWriter.FillInControlValues(_surface, buildArguments);
        }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            DisplayName = settingProperty.DisplayName;
            PropertyName = settingProperty.PropertyInfo.Name;
        }


        [UserInterfaceHint(Editor = Editor.Hidden)]
        public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();

        public string GetValue()
        {
            return _readWriter.GetValue();
        }

        public void SetValue(string newValue)
        {
            _readWriter.SetValue(newValue);
        }

        private class CompositeFormReadWriter
        {
            private readonly Type _compositeType;
            private readonly Control _surface;
            readonly JavaScriptSerializer _js = new JavaScriptSerializer();

            public CompositeFormReadWriter(Type compositeType,Control surface)
            {
                _compositeType = compositeType;
                _surface = surface;
            }

            public string GetValue()
            {
                var wrapper = new ConfigurationWrapper
                {
                    TypeName = _compositeType.AssemblyQualifiedName,
                    SerializedConfiguration = CmsFormReadWriter.ReadValuesFromControls(_surface)
                };

                return _js.Serialize(wrapper);
            }

            public void SetValue(string newValue)
            {
                if (!string.IsNullOrWhiteSpace(newValue))
                {
                    var wrapper = _js.Deserialize<ConfigurationWrapper>(newValue);
                    CmsFormReadWriter.FillInControlValues(_surface, wrapper.SerializedConfiguration);
                }

            }
        }

    }
}