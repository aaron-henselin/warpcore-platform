using BlazorComponents.Shared;
using Cms.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IDictionary<string,string> InitializeEditingSession(Guid formId, Guid? contentId)
        {
            var form = new FormRepository().FindContentVersions(By.ContentId(formId), WarpCore.Platform.Orm.ContentEnvironment.Live).Result.Single();
            var repo = RepositoryActivator.ActivateRepository<ISupportsCmsForms>(form.RepositoryUid);
            WarpCoreEntity entity;
            if (contentId != null)
                entity = repo.GetById(contentId.Value);
            else
                entity = repo.New();

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
                .Distinct();
                

            return entity.GetPropertyValues(x => linkedProperties.Contains(x.Name));
        }
    }
}
