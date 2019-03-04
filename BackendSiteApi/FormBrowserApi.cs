using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using BlazorComponents.Shared;
using Cms.Forms;
using WarpCore.Platform.DataAnnotations.Expressions;
using WarpCore.Platform.Orm;

namespace BackendSiteApi
{

    public class FormBrowserApiController : ApiController
    {
        [HttpGet]
        [Route("api/forms")]
        public List<FormModel> Page()
        {
            var drafts = new FormRepository().FindContentVersions(BooleanExpression.None, ContentEnvironment.Draft).Result
                .ToList();

            return drafts.Select(x => new FormModel {Name = x.Name, ContentId = x.ContentId}).ToList();
        }

    }


}