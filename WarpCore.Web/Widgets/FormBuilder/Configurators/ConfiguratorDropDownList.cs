using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Orm;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.FormBuilder
{
    [IncludeInToolbox(WidgetUid = ApiId, FriendlyName = "Option List", Category = "Form Controls")]
    public class ConfiguratorDropDownList: PlaceHolder, INamingContainer, IConfiguratorControl
    {
        public const string ApiId = "warpcore-formcontrol-dropdownlist";

        private ListControl _listControl = new DropDownList { CssClass = "form-control" };
        private ConfiguratorBuildArguments _buildArguments;

        [PropertiesAsOptionListSource]
        [UserInterfaceHint(Editor = Editor.OptionList)][DisplayName("Property")]
        public string PropertyName { get; set; }

        public void SetValue(string newValue)
        {
            _listControl.SelectedValue = newValue;
        }

        public string GetValue()
        {
            return _listControl.SelectedValue;
        }

        [UserInterfaceHint][DisplayName("Display Name")]
        [UserInterfaceBehavior(typeof(WhenPropertyNameChangedResetDisplayName))]
        public string DisplayName { get; set; }


        [UserInterfaceHint(Editor = Editor.CheckBox)][DisplayName("Required")]
        public bool IsRequired { get; set; }

        [UserInterfaceHint(Editor = Editor.Hidden)]
        public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();

        [DisplayName("Values From Repository")]
        [UserInterfaceHint(Editor = Editor.OptionList)]
        [RepositoryListControlSource]
        public string RepositoryApiId { get; set; }

        [DisplayName("Values From Entity")]
        [UserInterfaceHint(Editor = Editor.OptionList)]
        [EntitiesControlSource(nameof(RepositoryApiId))]
        public string EntityApiId { get; set; }



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
            _listControl.AutoPostBack = true; //todo, dependency.
            Page.RegisterRequiresControlState(_listControl);
            this.Controls.Add(_listControl);

        }

        public IEnumerable<ListOption> GetOptionsFromDataRelation(ConfiguratorBuildArguments buildArguments, string apiId)
        {
            var repoType = RepositoryTypeResolver.ResolveTypeByApiId(new Guid(apiId));
            var repo = (ISupportsCmsForms)Activator.CreateInstance(repoType);

            List<WarpCoreEntity> allItems=null;

            if (repo is IUnversionedContentRepository unversionedRepo)
            {
                allItems = unversionedRepo.FindContent(string.Empty)
                    .Cast<WarpCoreEntity>()
                    .ToList();
            }

            if (repo is IVersionedContentRepository versionedRepo)
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
                    Text = entity.Title,
                    Value = entity.ContentId.ToString()
                };

            }
        }

        public void ReloadListOptions(IDictionary<string,string> model)
        {
            var options = GetListOptions(_buildArguments,model);
            LoadNewOptions(options);

        }

        public void InitializeEditingContext(ConfiguratorBuildArguments buildArguments)
        {
            _buildArguments = buildArguments;
            ReloadListOptions(buildArguments.DefaultValues);

            buildArguments.Events.ValueChanged += (sender, args) => {
                ReloadListOptions(args.Model);
            };
        }



        private void LoadNewOptions(IReadOnlyCollection<ListOption> options)
        {
            var prevValue =
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

        private List<ListOption> GetListOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
        {
            var propertyToEdit = buildArguments.ClrType.GetProperty(PropertyName);
            var allAttributes = propertyToEdit.GetCustomAttributes(true);

            var customListSource = allAttributes.OfType<IListControlSource>().FirstOrDefault();
            var dataRelation = allAttributes.OfType<DataRelationAttribute>().FirstOrDefault();

            var options = new List<ListOption>();

            if (dataRelation != null)
                options = GetOptionsFromDataRelation(buildArguments, dataRelation.ApiId).ToList();

            if (customListSource != null)
                options = customListSource.GetOptions(buildArguments, model).ToList();
            return options;
        }


        protected override void LoadControlState(object savedState)
        {
            var savedOptions = (List<ListOption>) savedState;
            _listControl.Items.Clear();
            _listControl.Items.AddRange(savedOptions.Select(ListItemHelper.BuildListItem).ToArray());
        }

        protected override object SaveControlState()
        {
            var listOptions =
            _listControl.Items
                .Cast<ListItem>()
                .Select(x => new ListOption {Text = x.Text, Value = x.Value})
                .ToList();

            return listOptions;
        }
    }

    public static class ListItemHelper
    {
        public static ListItem BuildListItem(ListOption option)
        {
            return new ListItem(option.Text, option.Value);
        }
    }

}