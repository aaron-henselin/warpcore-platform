using System;
using System.Collections.Generic;
using WarpCore.Cms;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.Orm;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Orm;

namespace Cms.Forms
{
    [Table("cms_form")]
    [WarpCoreEntity(ApiId, TitleProperty = nameof(Name), ContentNameSingular = "Form")]
    public class CmsForm : VersionedContentEntity, IHasDesignedContent
    {
        public const string ApiId = "fb446b6d-3899-4ae1-8cf8-120044d6aa67";
        
        [Column]
        public string Name { get; set; }

        [SerializedComplexObject]
        public List<CmsPageContent> ChildNodes { get; set; } = new List<CmsPageContent>();

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
