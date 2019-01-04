using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Cms.Forms;
using Cms.Toolbox;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms.Routing;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web
{
    [ToolboxItem(WidgetUid = ApiId)]
    public class DynamicForm : Control, IHasInternalLayout
    {
        public const string ApiId = "wc-dynamic-form";

        private CmsForm _cmsForm;

        [UserInterfaceHint(Editor = Editor.OptionList), ContentControlSource(FormRepository.ApiId)]
        public Guid FormId { get; set; }


        public InternalLayout GetInternalLayout()
        {
            EnsureLayoutInitialized();

            var layout = new InternalLayout();
            layout.PlaceHolderIds.Add(nameof(surface));

            var formRepository = new FormRepository();
            _cmsForm = formRepository.FindContentVersions(By.ContentId(FormId), ContentEnvironment.Live).Result.Single();

            foreach (var item in _cmsForm.DesignedContent)              //todo: better way.
                item.PlacementContentPlaceHolderId = nameof(surface);

            layout.DefaultContent.AddRange(_cmsForm.DesignedContent);
            return layout;
        }

        private void EnsureLayoutInitialized()
        {
            if (Controls.Count > 0)
                return;

            FormTitleAdd = new HtmlGenericControl("h2");
            FormTitleEdit = new HtmlGenericControl("h2");
            FormTitleEditName = new HtmlGenericControl("span");
            surface = new PlaceHolder { ID = "surface" };

            this.Controls.Add(FormTitleAdd);
            this.Controls.Add(FormTitleEdit);
            this.Controls.Add(new PlaceHolder { ID = "surface" });

            var cancelButton = new Button { CssClass = "btn", Text = "Cancel" };
            cancelButton.Click += CancelButton_OnClickButton_OnClick;
            this.Controls.Add(cancelButton);

            var saveButton = new Button { CssClass = "btn", Text = "Save" };
            saveButton.Click += SaveButton_OnClick;
            this.Controls.Add(saveButton);
        }

        HtmlGenericControl FormTitleEditName { get; set; }

        HtmlGenericControl FormTitleEdit { get; set; }

         HtmlGenericControl FormTitleAdd { get; set; }


        [UserInterfaceIgnore]
        public Guid LayoutBuilderId => FormId;

        private ISupportsCmsForms _repo;

        private DynamicFormRequestContext _dynamicFormRequest;
        private IReadOnlyCollection<IConfiguratorControl> _activatedConfigurators;

        private PlaceHolder surface;


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureLayoutInitialized();

            //<h2 runat="server" ID="FormTitleAdd"></h2>
            //<h2 runat="server" ID="FormTitleEdit">Edit '<i><span runat="server" ID="FormTitleEditName"></span></i>'</h2>
            //<asp:PlaceHolder runat="server" ID="surface">

            //</asp:PlaceHolder>
            //<div>
            //    <asp:Button CssClass="btn" runat="server" ID="CancelButton" Text="Cancel" OnClick="CancelButton_OnClickButton_OnClick"/>
            //    <asp:Button CssClass="btn btn-primary" runat="server" ID="SaveButton" Text="Save" OnClick="SaveButton_OnClick"/>
            //</div>


            _dynamicFormRequest = Context.ToDynamicFormRequestContext();
            _repo = RepositoryActivator.ActivateRepository<ISupportsCmsForms>(_cmsForm.RepositoryUid);

            var draft = GetDraft();
            if (draft.IsNew)
            {
                var metadataRepo = new ContentTypeMetadataRepository();
                var metadata = metadataRepo.GetContentType(draft.GetType());
                FormTitleAdd.InnerText = $"Add {metadata.ContentNameSingular}";
                FormTitleEdit.Visible = false;
            }
            else
            {
                FormTitleAdd.Visible = false;
                FormTitleEditName.InnerText = draft.Title;
            }

            var d = draft.GetPropertyValues(ToolboxPropertyFilter.SupportsOrm);

            var configuratorEditingContext = new ConfiguratorBuildArguments
            {
                ClrType = draft.GetType(),
                PropertyFilter = ToolboxPropertyFilter.SupportsOrm,
                DefaultValues = d
            };
            //configuratorEditingContext.Events = CmsFormReadWriter.AddEventTracking(surface, configuratorEditingContext, _activatedConfigurators).Events;

            //CmsFormReadWriter.InitializeEditing(_activatedConfigurators, configuratorEditingContext);
            //SetConfiguratorEditingContextDefaultValuesFromUrl(configuratorEditingContext);
            //CmsFormReadWriter.SetDefaultValues(_activatedConfigurators, configuratorEditingContext);

        }


        private void SetConfiguratorEditingContextDefaultValuesFromUrl(ConfiguratorBuildArguments configuratorBuildArguments)
        {
            //var canSetDefaults = !Page.IsPostBack && _dynamicFormRequest.ContentId == null;
            //if (!canSetDefaults)
            //    return;

            var defaultValueCollection = _dynamicFormRequest.DefaultValues;
            foreach (var kvp in defaultValueCollection)
                configuratorBuildArguments.DefaultValues[kvp.Key] = kvp.Value;

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var eventTracking = CmsFormReadWriter.GetEventTracking(surface);
            eventTracking.RaiseEvents();
            eventTracking.UpdateEventData();
        }

        private WarpCoreEntity GetDraft()
        {
            if (_dynamicFormRequest.ContentId != null)
            {
                return _repo.GetById(_dynamicFormRequest.ContentId.Value);//FindContent(By.ContentId(_dynamicFormRequest.ContentId.Value), ContentEnvironment.Draft).Single();
            }
            else
                return _repo.New();

        }

        protected void SaveButton_OnClick(object sender, EventArgs e)
        {
            var draft = GetDraft();

            var newValues = CmsFormReadWriter.ReadValuesFromControls(_activatedConfigurators);
            draft.SetPropertyValues(newValues, x => true);

            _repo.Save(draft);

            HttpContext.Current.Response.Redirect("/admin");
        }

        protected void CancelButton_OnClickButton_OnClick(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Redirect("/admin");
        }

    }
}