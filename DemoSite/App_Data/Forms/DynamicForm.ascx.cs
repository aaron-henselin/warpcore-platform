using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Cms.Forms;
using Cms.Toolbox;
using Framework;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Widgets.FormBuilder;

namespace DemoSite
{
    public partial class DynamicForm : System.Web.UI.UserControl
    {
        private CmsForm _cmsForm;

        [Setting]
        public Guid FormId { get; set; }

        private Guid? _contentId;
        private IVersionedContentRepositoryBase _repo;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var formRepository = new FormRepository();
            _cmsForm = formRepository.FindContentVersions(By.ContentId(FormId),ContentEnvironment.Live).Result.Single();

            var repoManager = new RepositoryMetadataManager();
            var repoMetadata = repoManager.Find(nameof(RepositoryMetdata.RepositoryUid) + " eq '" +_cmsForm.RepositoryUid+"'").First();
            var repoType = Type.GetType(repoMetadata.AssemblyQualifiedTypeName);
            _repo = (IVersionedContentRepositoryBase)Activator.CreateInstance(repoType);
            

            CmsPageLayoutEngine.ActivateAndPlaceContent(surface, _cmsForm.DesignedContent);

            var contentIdRaw = Request["contentId"];
            if (!string.IsNullOrWhiteSpace(contentIdRaw))
                _contentId = new Guid(contentIdRaw);

            var draft = GetDraft();
            var d = draft.GetPropertyValues(ToolboxPropertyFilter.IsNotIgnoredType);

            var configuratorEditingContext = new ConfiguratorEditingContext
            {
                ClrType = draft.GetType(),
                PropertyFilter = ToolboxPropertyFilter.IsNotIgnoredType,
                CurrentValues = d
            };
            CmsFormReadWriter.PopulateListControls(surface, configuratorEditingContext);
            CmsFormReadWriter.FillInControlValues(surface,configuratorEditingContext);
            
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private CosmosEntity GetDraft()
        {
            CosmosEntity draft;
            if (_contentId != null)
                draft = _repo.FindContentVersions(By.ContentId(_contentId.Value), ContentEnvironment.Draft).Single();
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