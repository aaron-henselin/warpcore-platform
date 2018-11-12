using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.Cms;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Orm;

namespace Cms.Forms
{
    [Table("cms_form")]
    public class CmsForm : VersionedContentEntity, IHasDesignedLayout
    {
        [Column]
        public string Name { get; set; }

        [SerializedComplexObject]
        public List<CmsPageContent> FormContent { get; set; } = new List<CmsPageContent>();

        public List<CmsPageContent> DesignedContent => FormContent;
        public Guid DesignForContentId => ContentId;

        public Guid RepositoryUid { get; set; }
        public Guid ContentTypeId => RepositoryUid;
    }




    [ExposeToWarpCoreApi(ApiId)]
    public class FormRepository : VersionedContentRepository<CmsForm>
    {
        public const string ApiId = "d857f85b-4b44-490f-b989-1cfd11bd9960";
    }

}
