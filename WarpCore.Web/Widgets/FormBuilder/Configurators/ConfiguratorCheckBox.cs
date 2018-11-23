﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = ApiId, FriendlyName = "CheckBox", Category = "Form Controls")]
    public class ConfiguratorCheckBox : PlaceHolder, INamingContainer, ILabeledConfiguratorControl
    {
        public const string ApiId = "warpcore-formcontrol-checkBox";

        private CheckBox _checkbox = new CheckBox { CssClass = "form-control" };

        [UserInterfaceHint(Editor = Editor.OptionList)][FormControlPropertiesDataSource(typeof(bool))]
        public string PropertyName { get; set; }

        public void SetValue(string newValue)
        {
            bool outValue;
            var success = Boolean.TryParse(newValue, out outValue);
            if (success)
                _checkbox.Checked = outValue;
        
        }

        public string GetValue()
        {
            return _checkbox.Checked.ToString();
        }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            PropertyName = settingProperty.PropertyInfo.Name;
            DisplayName = settingProperty.DisplayName;
            Behaviors.AddRange(settingProperty.Behaviors.Select(x => x.AssemblyQualifiedName).ToList());
        }

        [UserInterfaceHint]
        [UserInterfaceBehavior(typeof(ResetDisplayNameWhenPropertyChanged))]
        public string DisplayName { get; set; }

        [UserInterfaceHint(Editor = Editor.Hidden)]
        public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();

        public bool Checked
        {
            get { return _checkbox.Checked; }
            set { _checkbox.Checked = value; }
        }

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;

            _checkbox.Text = DisplayName;
            //this.Controls.Add(new Label { Text = DisplayName });
            _checkbox.ID = PropertyName + "_CheckBox";
            this.Controls.Add(_checkbox);
        }


    }
}