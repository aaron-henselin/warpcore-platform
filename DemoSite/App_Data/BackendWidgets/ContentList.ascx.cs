
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Cms;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.UserInteraceHints;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace DemoSite
{
    public class ContentListConfiguration : ISupportsJavaScriptSerializer
    {
        public List<ContentListField> Fields { get; set; } = new List<ContentListField>();

        public Guid EditPage { get; set; }
        public Guid AddPage { get; set; }
    }

    public class ContentListField
    {
        public string Template { get; set; }
        public string Header { get; set; }
    }

    public class ContentListControlState
    {
        public List<ContentListField> Fields { get; set; } = new List<ContentListField>();
        public List<ContentListItem> Items { get; set; } = new List<ContentListItem>();
    }

    public class ContentListItem
    {
        public IDictionary<string, string> Values { get; set; }
    }

    public class ContentListConfiguratorSubForm : PlaceHolder
    {
        private Label _headerLabel;
        private Label _templateLabel;
        private Button _saveButton;

        public event EventHandler Saved;
        public string HeaderText { get; set; }
        public string TemplateText { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _headerLabel = new Label{Text = "Header"};
            this.Controls.Add(_headerLabel);
            this.Controls.Add(new TextBox());

            _templateLabel = new Label{Text="Template"};
            this.Controls.Add(_templateLabel);
            this.Controls.Add(new TextBox());

            _saveButton = new Button { Text = "Save" };
            _saveButton.Click += OnSave;
            this.Controls.Add(_saveButton);

        }

        private void OnSave(object sender, EventArgs e)
        {
            Saved?.Invoke(sender,e);
        }
    }

    [ToolboxItem(WidgetUid = "wc-content-list-configurator", FriendlyName ="Content List Configurator")]
    public class ContentListConfigurator : PlaceHolder, IConfiguratorControl
    {
        private IReadOnlyCollection<SettingProperty> _allProperties;

        private ContentListConfiguratorSubForm _subForm;
        private ContentListConfiguration _config;

        private HiddenField _editingField;
        private PlaceHolder _existingFieldsPlaceHolder;
        private PlaceHolder _subFormPlaceHolder;
        private Button _addButton;

        public string PropertyName { get; set; }

        //public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();
    

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _existingFieldsPlaceHolder = new PlaceHolder();
            this.Controls.Add(_existingFieldsPlaceHolder);

            _subFormPlaceHolder = new PlaceHolder();
            this.Controls.Add(_subFormPlaceHolder);

            _editingField = new HiddenField();
            this.Controls.Add(_editingField);
        }

        //public void InitializeEditingContext(ConfiguratorBuildArguments buildArguments)
        //{
        //    var currentRepositoryValue = buildArguments.DefaultValues.Get<Guid>(nameof(ContentList.RepositoryId));
        //    var entityType = RepositoryTypeResolver.ResolveTypeByApiId(currentRepositoryValue);
        //    _allProperties = ToolboxMetadataReader.ReadProperties(entityType, ToolboxPropertyFilter.SupportsOrm);


         
        //}

        private void SetupEditForm()
        {
            _subFormPlaceHolder.Controls.Clear();

            var isEditing = !string.IsNullOrEmpty(_editingField.Value);
            if (isEditing)
            {
                var subForm = new ContentListConfiguratorSubForm();
                _subFormPlaceHolder.Controls.Add(subForm);

                if ("Add" != _editingField.Value)
                {
                    var editNum = Convert.ToInt32(_editingField.Value);
                    var field = _config.Fields[editNum];

                    subForm.HeaderText = field.Header;
                    subForm.TemplateText = field.Template;
                }

            }

            _addButton.Visible = !isEditing;
        }

        public override void DataBind()
        {
            base.DataBind();

            _existingFieldsPlaceHolder.Controls.Clear();

            for (var index = 0; index < _config.Fields.Count; index++)
            {
                var field = _config.Fields[index];
                var readonlyLabel = new Label {Text = field.Header};
                _existingFieldsPlaceHolder.Controls.Add(readonlyLabel);

                var editButton = new Button {Text = "Edit", CommandArgument = index.ToString()};
                _existingFieldsPlaceHolder.Controls.Add(editButton);
            }

            _addButton = new Button {Text = "Add", CommandArgument = "Add"};
            _addButton.Click += (sender, args) =>
            {
                _editingField.Value = ((Button) sender).CommandArgument; 
                SetupEditForm();
            };
            this.Controls.Add(_addButton);
            SetupEditForm();
            
        }

        public void SetConfiguration(SettingProperty settingProperty)
        {
            this.PropertyName = settingProperty.PropertyInfo.Name;
            //Behaviors.AddRange(settingProperty.Behaviors.Select(x => x.AssemblyQualifiedName).ToList());
        }

        public void SetValue(string newValue)
        {
            _config = ExtensibleTypeConverter.ChangeType<ContentListConfiguration>(newValue);
            DataBind();
        }

        public string GetValue()
        {
            return ExtensibleTypeConverter.ChangeType<string>(_config);
        }

    }

    [ToolboxItem(AscxPath = "/App_Data/BackendWidgets/ContentList.ascx", WidgetUid = "wc-content-list", FriendlyName = "Content List")]
    public partial class ContentList : System.Web.UI.UserControl
    {
        public const string ApiId = "wc-content-list";

        [UserInterfaceHint(Editor = Editor.OptionList)]
        [DataRelation(RepositoryMetadataManager.ApiId)]
        public Guid RepositoryId { get; set; }

        [UserInterfaceHint(CustomEditorType = nameof(ContentListConfigurator))]
        public ContentListConfiguration Config { get; set; }

        //private ContentListControlState _controlState = new ContentListControlState();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);
            Reload();

            this.DataBind();

            CreateNewItemButton.Visible = Config.AddPage != Guid.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
          
        }

        private void Reload()
        {
            var repoType = RepositoryTypeResolver.ResolveTypeByApiId(RepositoryId);
            var repo = (IVersionedContentRepository) Activator.CreateInstance(repoType);
            var allDrafts = repo.FindContentVersions(string.Empty, ContentEnvironment.Draft).ToList();


            List<object> ds = new List<object>();
            var fields = Config.Fields;

            if (!Config.Fields.Any())
            {
                Config.Fields = new List<ContentListField>();

                var titleField = new ContentListField
                {
                    Header = "Title",
                    Template = Templating.CreateToStringExpression(nameof(WarpCoreEntity.Title))
                };
                Config.Fields.Add(titleField);
            }

            foreach (var d in allDrafts)
            {          
                var row = new List<string>();
                var dict1 = d.GetPropertyValues(x => true);
                foreach (var f in fields)
                {
                    var val = Templating.Interpolate(f.Template, dict1);
                    row.Add(val);
                }

                ds.Add(row);
            }

            var fieldConfigurations = new List<object>();
            foreach (var f in fields)
            {
                fieldConfigurations.Add(new { title = f.Header });
            }


            var js = new JavaScriptSerializer();
            Data.Value = js.Serialize(ds);
            Fields.Value = js.Serialize(fieldConfigurations);
            DataBind();

            //this.ContentListDataGrid.DataSource = ds;

            

            //this.ContentListDataGrid.AutoGenerateColumns = false;
            //foreach (var field in fields)
            //{
            //    this.ContentListDataGrid.Columns.Add(new BoundColumn { DataField = field, HeaderText = field });
            //}

            //this.ContentListDataGrid.DataBind();

            //var configFieldLookup = Configuration.Fields
            //    .Select(x => x.PropertyName)
            //    .ToDictionary(x => x, x => x);

            //foreach (var draft in allDrafts)
            //{
            //    var li = new ContentListItem
            //    {
            //        Values = draft.GetPropertyValues(x => configFieldLookup.ContainsKey(x.Name))
            //    };
            //    _controlState.Items.Add(li);
            //}
        }

        protected void CreateNewItemButton_OnClick(object sender, EventArgs e)
        {
            Guid defaultSiteId = Guid.Empty;
            var defaultFrontendSite = SiteManagementContext.GetSiteToManage();
            if (defaultFrontendSite != null)
                defaultSiteId = defaultFrontendSite.ContentId;

            var uriBuilderContext = HttpContext.Current.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            var editPage = new CmsPageRepository()
                .FindContentVersions(By.ContentId(Config.AddPage), ContentEnvironment.Live)
                .Result
                .Single();

            var defaultValues = new DefaultValueCollection {["SiteId"] = defaultSiteId.ToString()};
            var newPageUri = uriBuilder.CreateUri(editPage, UriSettings.Default, new Dictionary<string, string>
            {
                [nameof(DynamicFormRequestContext.DefaultValues)] = defaultValues.ToString()
            });
            Response.Redirect(newPageUri.PathAndQuery);


        }
    }
}