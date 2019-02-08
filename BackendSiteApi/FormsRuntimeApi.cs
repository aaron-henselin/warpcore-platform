using BlazorComponents.Shared;
using Cms.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace BackendSiteApi
{
    public class FormsRuntimeApiController : ApiController
    {
        [HttpGet]
        [Route("api/forms-runtime/forms/{formId}/description")]
        public ConfiguratorFormDescription GetConfiguratorForm(Guid formId)
        {
            var form = new FormRepository().FindContentVersions(By.ContentId(formId),WarpCore.Platform.Orm.ContentEnvironment.Live).Result.Single();
            var description =
                new ConfiguratorFormDescription
                {
                    Layout = new StructureNodeConverter().GetPageStructure(form),
                    DefaultValues = new Dictionary<string, string>()
                };
            return description;
        }

       


        [HttpPost]
        [Route("api/forms-runtime/session")]
        public EditingSession InitializeEditingSession(Guid formId, Guid? contentId)
        {
            var form = new FormRepository().FindContentVersions(By.ContentId(formId), WarpCore.Platform.Orm.ContentEnvironment.Live).Result.Single();
            var repo = RepositoryActivator.ActivateRepository<ISupportsCmsForms>(form.RepositoryUid);
            WarpCoreEntity entity;
            if (contentId != null)
                entity = repo.GetById(contentId.Value);
            else
                entity = repo.New();


            var runtime = new FormsRuntime();
            var editingScope = runtime.BuildObjectEditingScope(form);
            var entityType = entity.GetType();
            var initialValues = entity.GetPropertyValues(x => editingScope.Contains(x.Name));

            return runtime.EditingSession(initialValues,editingScope,entityType);

            

        }
    }

    public class FormsRuntime
    {
        public IReadOnlyCollection<string> BuildObjectEditingScope(CmsForm form)
        {
            //todo: better options??
            var toolboxManager = new ToolboxManager();
            var widgetsOnPage = form
                .GetAllDescendents()
                .Where(x => x.WidgetTypeCode != null);

            var justClientSideWidgets =
                widgetsOnPage.Where(x => toolboxManager.GetToolboxItemByCode(x.WidgetTypeCode).UseClientSidePresentationEngine);

            var linkedProperties = justClientSideWidgets
                .Where(x => x.Parameters.ContainsKey(nameof(BlazorToolboxItem.PropertyName)))
                .Select(x => x.Parameters[nameof(BlazorToolboxItem.PropertyName)])
                .Distinct()
                .ToList();

            return linkedProperties;
        }



        public EditingSession EditingSession(IDictionary<string, string> initialValues, IReadOnlyCollection<string> editingScope, Type entityType)
        {
            var session = new EditingSession();
            session.InitialValues = initialValues;

            var properties = ToolboxMetadataReader.ReadProperties(entityType, x => editingScope.Contains(x.Name));
            var needsDataSource = properties.Where(x => x.Editor == Editor.OptionList);
            foreach (var property in needsDataSource)
            {
                var entities = GetDataRelationshipEntities(property);
                var asDataSourceItems =
                    entities.Select(x => new DataSourceItem {Name = x.Title, Value = x.ContentId.ToString()}).ToList();
                var ds = new LocalDataSource {Items = asDataSourceItems};
                session.LocalDataSources.Add(property.PropertyInfo.Name, ds);
            }

            return session;
        }

        public IReadOnlyCollection<WarpCoreEntity> GetDataRelationshipEntities(SettingProperty settingProperty)
        {
            var info = settingProperty.PropertyInfo;

            var dataRelation = info.GetCustomAttribute<DataRelationAttribute>();
            if (dataRelation == null)
                return new List<WarpCoreEntity>(); ;

            var repo = RepositoryActivator.ActivateRepository(new Guid(dataRelation.ApiId));
            List<WarpCoreEntity> allItems = null;

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


            return allItems;
        }
    }
}
