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

    [Table("cms_content_list_definition")]
    [WarpCoreEntity(ApiId, TitleProperty = nameof(Name), ContentNameSingular = "List")]
    public class CmsContentListDefinition : VersionedContentEntity
    {
        public const string ApiId = "7b04751b-580a-4313-9f51-ce9982a3bd4e";

        [Column]
        public string Name { get; set; }
        
        [Column]
        [DataRelation(RepositoryMetadataManager.ApiId)]
        public Guid RepositoryUid { get; set; }

        [Column]
        public string Filter { get; set; }

        [SerializedComplexObject]
        public List<CmsListField> Fields { get; set; } = new List<CmsListField>();
    }

    public class CmsListField
    {
        public string Label { get; set; }

        public string PropertyName { get; set; }
        public Guid Id { get; set; }
    }


    [ExposeToWarpCoreApi(ApiId)]
    public class FormRepository : VersionedContentRepository<CmsForm>
    {
        public const string ApiId = "d857f85b-4b44-490f-b989-1cfd11bd9960";
    }

    [ExposeToWarpCoreApi(ApiId)]
    public class ContentListDefinitionRepository : VersionedContentRepository<CmsContentListDefinition>
    {
        public const string ApiId = "c27d78ee-1bf8-48d8-962d-891a8934b361";
    }

}
