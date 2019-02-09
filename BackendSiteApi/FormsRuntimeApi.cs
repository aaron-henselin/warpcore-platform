using BlazorComponents.Shared;
using Cms.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WarpCore.Cms;
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
            return runtime.EditingSession(form, entity);

            

        }
    }

    public class FormsRuntime
    {
        public IReadOnlyCollection<CmsPageContent> GetClientSideWidets(CmsForm form)
        {
            //todo: better options??
            var toolboxManager = new ToolboxManager();
            var widgetsOnPage = form
                .GetAllDescendents()
                .Where(x => x.WidgetTypeCode != null);

          return
                widgetsOnPage.Where(x => toolboxManager.GetToolboxItemByCode(x.WidgetTypeCode).UseClientSidePresentationEngine).ToList();

        }

        public ILookup<string, CmsPageContent> GetClientSideWidgetLookup(CmsForm form)
        {
            var clientSide = GetClientSideWidets(form);
            return clientSide
                .Where(x => x.Parameters.ContainsKey(nameof(BlazorToolboxItem.PropertyName)))
                .ToLookup(x => x.Parameters[nameof(BlazorToolboxItem.PropertyName)]);

        }

        public IReadOnlyCollection<string> BuildObjectEditingScope(CmsForm form)
        {
            return GetClientSideWidgetLookup(form).Select(x => x.Key).Distinct().ToList();
        }


        public EditingSession EditingSession(CmsForm form, WarpCoreEntity entity)
        {
            var editingScope = BuildObjectEditingScope(form);
            var initialValues = entity.GetPropertyValues(x => editingScope.Contains(x.Name));
            return EditingSession(form, entity.GetType(), initialValues);
        }

        public EditingSession EditingSession(CmsForm form, Type entityType, IDictionary<string, string> initialValues)
        {
            var session = new EditingSession {InitialValues = initialValues};

            var widgetsGroupedByProperty = GetClientSideWidgetLookup(form);
            var editingScope = BuildObjectEditingScope(form);

            var properties = ToolboxMetadataReader.ReadProperties(entityType, x => editingScope.Contains(x.Name)).ToDictionary(x => x.PropertyInfo.Name);

            
            foreach (var widgetGroup in widgetsGroupedByProperty)
            {
                var propertyName = widgetGroup.Key;
                var widgetsNeedingDataSource = widgetGroup.ToList();

                foreach (var widget in widgetsNeedingDataSource)
                {
                  
                    var entities = GetDataRelationshipEntities(properties[propertyName]);
                    var asDataSourceItems = entities.Select(x => new DataSourceItem { Name = x.Title, Value = x.ContentId.ToString() }).ToList();
                    var ds = new LocalDataSource { Items = asDataSourceItems };
                    session.LocalDataSources.Add(propertyName, ds);
                }



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
