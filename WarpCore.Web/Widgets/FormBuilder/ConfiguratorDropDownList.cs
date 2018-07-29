using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorDropDownList", FriendlyName = "Option List", Category = "Form Controls")]
    public class ConfiguratorDropDownList: PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private ListControl _listControl = new DropDownList { CssClass = "form-control" };

        [Setting(SettingType = SettingType.OptionList)]
        [PropertyListControlSource]
        public string PropertyName { get; set; }

        [Setting]
        public string DisplayName { get; set; }

        public string SelectedValue
        {
            get { return _listControl.SelectedValue; }
            set { _listControl.SelectedValue = value; }
        }


        [Setting(SettingType = SettingType.OptionList)]
        [RepositoryListControlSource]
        public string RepositoryUid { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;
            
            this.Controls.Add(new Label { Text = DisplayName });
            _listControl.ID = PropertyName + "_ListControl";
            this.Controls.Add(_listControl);

            //var metadataManager = new RepositoryMetadataManager();
            //var repo = metadataManager.Find().SingleOrDefault(x => x.RepositoryUid == RepositoryUid);
            //if (repo != null)

            
        }

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
            var prop = editingContext.ClrType.GetProperty(PropertyName);
            var dd = prop
                .GetCustomAttributes(true)
                .OfType<IListControlSource>()
                .FirstOrDefault();

            var prevValue=
                _listControl.SelectedValue;
            _listControl.Items.Clear();
            var options = dd.GetOptions(editingContext);
            foreach (var item in options)
                _listControl.Items.Add(new ListItem(item.Text, item.Value));

            var prevValueStillExists = _listControl.Items.Cast<ListItem>().Any(x => x.Value == prevValue);
            if (prevValueStillExists)
                _listControl.SelectedValue = prevValue;
        }
    }
}