using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Orm;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = "WC/ConfiguratorDropDownList", FriendlyName = "Option List", Category = "Form Controls")]
    public class ConfiguratorDropDownList: PlaceHolder, INamingContainer, IConfiguratorControl
    {
        private ListControl _listControl = new DropDownList { CssClass = "form-control" };

        [FormControlPropertiesDataSource]
        [Setting(SettingType = SettingType.OptionList)][DisplayName("Property")]
        public string PropertyName { get; set; }

        public void SetValue(string newValue)
        {
            _listControl.SelectedValue = newValue;
        }

        public string GetValue()
        {
            return _listControl.SelectedValue;
        }

        [Setting][DisplayName("Display Name")]
        public string DisplayName { get; set; }

        [Setting(SettingType = SettingType.CheckBox)][DisplayName("Required")]
        public bool IsRequired { get; set; }



        [Setting(SettingType = SettingType.OptionList)]
        [RepositoryListControlSource]
        public string RepositoryUid { get; set; }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            PropertyName = settingProperty.PropertyInfo.Name;
            DisplayName = settingProperty.DisplayName;
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;
            
            this.Controls.Add(new Label { Text = DisplayName, CssClass = "form-label" });
            _listControl.ID = PropertyName + "_ListControl";
            this.Controls.Add(_listControl);

        }

        public IEnumerable<ListOption> GetOptionsFromDataRelation(ConfiguratorEditingContext editingContext, string apiId)
        {
            var repoType = RepositoryTypeResolver.ResolveTypeByApiId(new Guid(apiId));
            var repo = (IContentRepository)Activator.CreateInstance(repoType);

            List<WarpCoreEntity> allItems=null;

            if (repo is IUnversionedContentRepositoryBase unversionedRepo)
            {
                allItems = unversionedRepo.FindContent(string.Empty)
                    .Cast<WarpCoreEntity>()
                    .ToList();
            }

            if (repo is IVersionedContentRepositoryBase versionedRepo)
            {
                allItems = versionedRepo.FindContentVersions(string.Empty)
                    .Cast<WarpCoreEntity>()
                    .ToList();
            }

            if (allItems == null)
                throw new Exception();

            foreach (var entity in allItems)
            {
                yield return new ListOption
                {
                    Text = entity.ToString(),
                    Value = entity.ContentId.ToString()
                };

            }
        }

        public void InitializeEditingContext(ConfiguratorEditingContext editingContext)
        {
            var propertyToEdit = editingContext.ClrType.GetProperty(PropertyName);
            var allAttributes = propertyToEdit.GetCustomAttributes(true);

            var customListSource = allAttributes.OfType<IListControlSource>().FirstOrDefault();
            var dataRelation = allAttributes.OfType<DataRelationAttribute>().FirstOrDefault();

            var options = new List<ListOption>();

            if (dataRelation != null)
                options = GetOptionsFromDataRelation(editingContext,dataRelation.ApiId).ToList();

            if (customListSource != null)
                options = customListSource.GetOptions(editingContext).ToList();
            


            var prevValue=
                _listControl.SelectedValue;
            _listControl.Items.Clear();
            
            foreach (var item in options)
                _listControl.Items.Add(new ListItem(item.Text, item.Value));

            var prevValueStillExists = _listControl.Items.Cast<ListItem>().Any(x => x.Value == prevValue);
            if (prevValueStillExists)
                _listControl.SelectedValue = prevValue;
            else
            {
                prevValue = string.Empty;
                prevValueStillExists = _listControl.Items.Cast<ListItem>().Any(x => x.Value == prevValue);
                if (prevValueStillExists)
                    _listControl.SelectedValue = prevValue;
                else if (_listControl.Items.Count > 0)
                    _listControl.SelectedValue = _listControl.Items[0].Value;
            }


        }
        
    }
}