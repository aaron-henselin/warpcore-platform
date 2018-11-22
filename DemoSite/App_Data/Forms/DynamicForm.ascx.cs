using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Cms.Forms;
using Cms.Toolbox;
using WarpCore.Cms.Routing;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets.FormBuilder;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace DemoSite
{
    

    public partial class DynamicForm : System.Web.UI.UserControl
    {
        private CmsForm _cmsForm;

        [UserInterfaceHint]
        public Guid FormId { get; set; }
        
        private IVersionedContentRepositoryBase _repo;

        private DynamicFormRequestContext _dynamicFormRequest;
        private ConfiguratorEvents _configuratorEvents;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _dynamicFormRequest = Context.ToDynamicFormRequestContext();
            _configuratorEvents = new ConfiguratorEvents();

            var formRepository = new FormRepository();
            _cmsForm = formRepository.FindContentVersions(By.ContentId(FormId),ContentEnvironment.Live).Result.Single();

            var repoManager = new RepositoryMetadataManager();
            var repoMetadata = repoManager.GetRepositoryMetdataByTypeResolverUid(_cmsForm.RepositoryUid);
            var repoType = Type.GetType(repoMetadata.AssemblyQualifiedTypeName);
            _repo = (IVersionedContentRepositoryBase)Activator.CreateInstance(repoType);

            CmsPageLayoutEngine.ActivateAndPlaceContent(surface, _cmsForm.DesignedContent);


            var draft = GetDraft();
            var d = draft.GetPropertyValues(ToolboxPropertyFilter.SupportsOrm);

            var configuratorEditingContext = new ConfiguratorEditingContext
            {
                ClrType = draft.GetType(),
                PropertyFilter = ToolboxPropertyFilter.SupportsOrm,
                CurrentValues = d,
                Events = _configuratorEvents
            };
            CmsFormReadWriter.PopulateListControls(surface, configuratorEditingContext);
            SetConfiguratorEditingContextDefaultValuesFromUrl(configuratorEditingContext);
            CmsFormReadWriter.FillInControlValues(surface,configuratorEditingContext);
            CmsFormReadWriter.AddEventTracking(surface, configuratorEditingContext);
        }


        private void SetConfiguratorEditingContextDefaultValuesFromUrl(ConfiguratorEditingContext configuratorEditingContext)
        {
            var canSetDefaults = !Page.IsPostBack && _dynamicFormRequest.ContentId == null;
            if (!canSetDefaults)
                return;

            var defaultValueCollection = _dynamicFormRequest.DefaultValues;
            foreach (var kvp in defaultValueCollection)
                configuratorEditingContext.CurrentValues[kvp.Key] = kvp.Value;

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var values = CmsFormReadWriter.GetChangedValues(surface);
            foreach (var value in values)
                _configuratorEvents.RaiseValueChanged(value);
        }

        private WarpCoreEntity GetDraft()
        {
            WarpCoreEntity draft;
            if (_dynamicFormRequest.ContentId != null)
                draft = _repo.FindContentVersions(By.ContentId(_dynamicFormRequest.ContentId.Value), ContentEnvironment.Draft).Single();
            else
                draft = _repo.New();

            return draft;
        }

        protected void SaveButton_OnClick(object sender, EventArgs e)
        {
            var draft = GetDraft();

            var newValues = CmsFormReadWriter.ReadValuesFromControls(surface);
            draft.SetPropertyValues(newValues, x => true);

            _repo.Save(draft);
            
            Response.Redirect("/admin");
        }

        protected void CancelButton_OnClickButton_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("/admin");
        }
    }
}