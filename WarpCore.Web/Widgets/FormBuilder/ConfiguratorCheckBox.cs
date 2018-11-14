using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorCheckBox", FriendlyName = "CheckBox", Category = "Form Controls")]
    public class ConfiguratorCheckBox : PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private CheckBox _checkbox = new CheckBox { CssClass = "form-control" };

        [Setting]
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

        [Setting]
        public string DisplayName { get; set; }

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