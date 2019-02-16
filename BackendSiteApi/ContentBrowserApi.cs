using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using BlazorComponents.Shared;
using Cms.Forms;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace BackendSiteApi
{
    public class ContentBrowserApi : ApiController
    {
        [HttpGet]
        [Route(ContentBrowserApiRoutes.ListDescription)]
        public ContentListDescription ListDescription(Guid listId)
        {
            var definition = new ContentListDefinitionRepository().FindContentVersions(By.ContentId(listId)).Result
                .Single();
           
            return new ContentListDescription
            {
                Fields = definition.Fields.Select(x => new ListField
                {
                    FieldId = x.Id,
                    DisplayName = x.Label,
                    PropertyName = x.PropertyName

                }).ToList()
            };
        }

        [HttpGet]
        [Route(ContentBrowserApiRoutes.ListDataFetch)]
        public ContentListData GetContentListData(Guid listId, string filter)
        {
            var definition = new ContentListDefinitionRepository().FindContentVersions(By.ContentId(listId)).Result
                .Single();

            var repo = RepositoryActivator.ActivateRepository(definition.RepositoryUid);
            List<WarpCoreEntity> allItems = null;

            if (repo is IUnversionedContentRepository unversionedRepo)
            {
                allItems = unversionedRepo.FindContent(filter)
                    .Cast<WarpCoreEntity>()
                    .ToList();
            }

            if (repo is IVersionedContentRepository versionedRepo)
            {
                allItems = versionedRepo.FindContentVersions(filter)
                    .Cast<WarpCoreEntity>()
                    .ToList();
            }

            if (allItems == null)
                throw new Exception();

            var propertyNameLookup = definition.Fields.ToLookup(x => x.PropertyName);


            var wrapper = new ContentListData();
            foreach (var item in allItems)
            {
                var dict = item.GetPropertyValues(x => propertyNameLookup.Contains(x.Name));
                wrapper.Items.Add(dict);
            }


            return wrapper;
        }
    }



}
