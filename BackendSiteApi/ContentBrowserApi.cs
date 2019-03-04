using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using BlazorComponents.Shared;
using Cms.Forms;
using WarpCore.Platform.DataAnnotations.Expressions;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace BackendSiteApi
{
    public class ContentBrowserApiController : ApiController
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
        public ContentListData GetContentListData(Guid repositoryApiId, Guid listId,string filter)
        {
            var definition = new ContentListDefinitionRepository().FindContentVersions(By.ContentId(listId)).Result
                .Single();

            var repo = RepositoryActivator.ActivateRepository(repositoryApiId);
            List<WarpCoreEntity> allItems = null;

            var entityType = RepositoryTypeResolver.ResolveTypeByApiId(repositoryApiId);
            var tokens = new BooleanExpressionParser().ReadBooleanExpressionTokens(filter);
            var expression = new BooleanExpressionTokenReader().CreateBooleanExpressionFromTokens(tokens);

            


            if (repo is IUnversionedContentRepository unversionedRepo)
            {
                allItems = unversionedRepo.FindContent(expression)
                    .Cast<WarpCoreEntity>()
                    .ToList();
            }

            if (repo is IVersionedContentRepository versionedRepo)
            {
                allItems = versionedRepo.FindContentVersions(expression, ContentEnvironment.Draft)
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
                dict[nameof(item.ContentId)] = item.ContentId.ToString();
                wrapper.Items.Add(dict);
            }


            return wrapper;
        }
    }



}
