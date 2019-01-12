using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.FormBuilder
{
    public class WhenPropertyNameChangedResetDisplayName : IUserInterfaceBehavior
    {
        private ILabeledConfiguratorControl _control;
        private Type _clrType;
        

        public void RegisterBehavior(IConfiguratorControl control, ConfiguratorBuildArguments buildArguments)
        {
            _control = (ILabeledConfiguratorControl)control;
            buildArguments.Events.ValueChanged += EventsOnValueChanged;

            _clrType = ConfiguratorEditingContextHelper.GetClrType(buildArguments.ParentEditingContext);
        }

        private void EventsOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var wasPropertyNameChanged = nameof(IConfiguratorControl.PropertyName) == e.PropertyName;
            if (!wasPropertyNameChanged)
                return;

            var propertyMetadata =
            ToolboxMetadataReader.GetPropertyMetadata(_clrType,e.NewValue);

            _control.SetValue(propertyMetadata.DisplayName);
        }
    }


    [global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId,FriendlyName = "TextBox", Category = "Form Controls")]
    public class ConfiguratorTextBox : PlaceHolder, INamingContainer, ILabeledConfiguratorControl
    {
        public const string ApiId = "warpcore-formcontrol-textbox";

        private TextBox _tbx = new TextBox { CssClass = "form-control"};

        /// <summary>
        /// Sets which Property of the object should be configured using this textbox,
        /// when a value is submitted by an end user.
        /// </summary>
        [PropertiesAsOptionListSource(typeof(string),typeof(int),typeof(decimal))]
        [UserInterfaceHint(Editor = Editor.OptionList)][DisplayName("Property name")]
        public string PropertyName { get; set; }

        [DisplayName("TextBox Type")]
        [UserInterfaceHint(Editor = Editor.OptionList)]
        [TextBoxModesDataSource]
        public string TextBoxMode { get; set; }

        /// <summary>
        /// Sets a custom label for the textbox when presented to the end user.
        /// </summary>
        [UserInterfaceHint][DisplayName("Display name")]
        [UserInterfaceBehavior(typeof(WhenPropertyNameChangedResetDisplayName))]
        public string DisplayName { get; set; }


        [UserInterfaceHint(Editor = Editor.Hidden)]
        public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();

        /// <summary>
        /// Determines whether or not entry of the textbox must be supplied by an end user.
        /// </summary>
        [UserInterfaceHint(Editor = Editor.CheckBox)]
        [DisplayName("Required")]
        public bool IsRequired { get; set; }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            PropertyName = settingProperty.PropertyInfo.Name;
            DisplayName = settingProperty.DisplayName;

            if (new[] {typeof(int), typeof(decimal)}.Contains(settingProperty.PropertyInfo.PropertyType))
                TextBoxMode = System.Web.UI.WebControls.TextBoxMode.Number.ToString();

            if (new[] {typeof(DateTime)}.Contains(settingProperty.PropertyInfo.PropertyType))
                TextBoxMode = System.Web.UI.WebControls.TextBoxMode.DateTime.ToString();

            if (Editor.RichText == settingProperty.Editor)
                TextBoxMode = System.Web.UI.WebControls.TextBoxMode.MultiLine.ToString();

            Behaviors.AddRange(settingProperty.Behaviors.Select(x => x.AssemblyQualifiedName).ToList());
        }


        public void InitializeEditingContext(ConfiguratorBuildArguments buildArguments)
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

            if (_tbx.TextMode == System.Web.UI.WebControls.TextBoxMode.MultiLine)
                _tbx.CssClass = "edit-with-rte";

            this.Controls.Add(_tbx);

            var rtePanel = new Panel {CssClass = "rte-surface"};
            rtePanel.Attributes.Add("data-rte-for",_tbx.ClientID);
            this.Controls.Add(rtePanel);

        }


        private class TextBoxModesDataSourceAttribute : Attribute, IListControlSource
        {
            public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
            {
                return Enum.GetNames(typeof(TextBoxMode)).Select(x => new ListOption { Text = x, Value = x }).ToList();
            }
        }

    }


}