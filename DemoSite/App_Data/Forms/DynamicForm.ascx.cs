﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Forms;
using Cms.Toolbox;
using WarpCore.Cms.Routing;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
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
        public const string ApiId = "wc-dynamic-form";

        private CmsForm _cmsForm;

        [UserInterfaceHint(Editor = Editor.OptionList), ContentControlSource(FormRepository.ApiId)]
        public Guid FormId { get; set; }
        
        private IContentRepository _repo;

        private DynamicFormRequestContext _dynamicFormRequest;
        private IReadOnlyCollection<IConfiguratorControl> _activatedConfigurators;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _dynamicFormRequest = Context.ToDynamicFormRequestContext();

            var formRepository = new FormRepository();
            _cmsForm = formRepository.FindContentVersions(By.ContentId(FormId),ContentEnvironment.Live).Result.Single();

            //var repoManager = new RepositoryMetadataManager();
            //var repoMetadata = repoManager.GetRepositoryMetdataByTypeResolverUid(_cmsForm.RepositoryUid);
            //var repoType = Type.GetType(repoMetadata.AssemblyQualifiedTypeName);
            //_repo = (IVersionedContentRepository)Activator.CreateInstance(repoType);
            _repo = RepositoryActivator.ActivateRepository(_cmsForm.RepositoryUid);

            _activatedConfigurators = CmsPageLayoutEngine.ActivateAndPlaceContent(surface, _cmsForm.DesignedContent).OfType<IConfiguratorControl>().ToList();


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
                FormTitleEditName.InnerText = draft.GetTitle();
            }
            
            var d = draft.GetPropertyValues(ToolboxPropertyFilter.SupportsOrm);

            var configuratorEditingContext = new ConfiguratorBuildArguments
            {
                ClrType = draft.GetType(),
                PropertyFilter = ToolboxPropertyFilter.SupportsOrm,
                DefaultValues = d
            };
            configuratorEditingContext.Events=CmsFormReadWriter.AddEventTracking(surface, configuratorEditingContext,_activatedConfigurators).Events;
            
            CmsFormReadWriter.InitializeEditing(_activatedConfigurators, configuratorEditingContext);
            SetConfiguratorEditingContextDefaultValuesFromUrl(configuratorEditingContext);
            CmsFormReadWriter.SetDefaultValues(_activatedConfigurators, configuratorEditingContext);
            
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
                return _repo.FindContent(By.ContentId(_dynamicFormRequest.ContentId.Value), ContentEnvironment.Draft).Single();
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
            
            Response.Redirect("/admin");
        }

        protected void CancelButton_OnClickButton_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("/admin");
        }
    }
}