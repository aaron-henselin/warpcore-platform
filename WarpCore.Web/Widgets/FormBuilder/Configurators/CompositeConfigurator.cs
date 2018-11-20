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

        [UserInterfaceHint(Editor = Editor.Text)]
        public Type CompositeType { get; set; }
        public string DisplayName { get; set; }

        private class ConfigurationWrapper
        {
            public string TypeName { get; set; }
            public Dictionary<string,string> SerializedConfiguration { get; set; }
        }

        private PlaceHolder _surface;
        private CompositeFormReadWriter _readWriter;

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
            _surface = new PlaceHolder();
            _surface.Controls.Add(new Label{Text=DisplayName, CssClass = "form-label" });
            _surface.Controls.Add(new RuntimeContentPlaceHolder{ PlaceHolderId = "FormBody" });
            this.Controls.Add(_surface);

            _readWriter = new CompositeFormReadWriter(CompositeType, _surface);

            var cmsForm = ConfiguratorFormBuilder.GenerateDefaultForm(CompositeType);
            CmsPageLayoutEngine.ActivateAndPlaceContent(_surface, cmsForm.DesignedContent);
            CmsFormReadWriter.PopulateListControls(_surface, editingContext);
            //CmsFormReadWriter.FillInControlValues(_surface, editingContext);
        }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            DisplayName = settingProperty.DisplayName;
            CompositeType = settingProperty.PropertyInfo.PropertyType;
            PropertyName = settingProperty.PropertyInfo.Name;
        }



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