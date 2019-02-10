using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.Orm;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility.DynamicContent
{
   

    public class ContentInterfaceRepository : UnversionedContentRepository<ContentInterface>
    {
        public ContentInterface GetCustomFieldsTypeExtension(Guid uid)
        {
            return Find().Single(x => x.ContentTypeId == uid && x.InterfaceName == KnownTypeExtensionNames.CustomFields);
        }
    }

    

    [WarpCoreEntity(ApiId, TitleProperty = nameof(InterfaceName), ContentNameSingular = "Content Interface",SupportsCustomFields = false)]
    [Table("cms_content_type_interface")]
    public class ContentInterface : UnversionedContentEntity
    {
        public const string ApiId = "f93e2357-bb01-43b0-b1d7-c0f59990fc5e";
        public Guid ContentTypeId { get; set; }

        public string InterfaceName { get; set; }

        [SerializedComplexObject]
        public List<InterfaceField> InterfaceFields { get; set; } = new List<InterfaceField>();
    }



    public struct KnownTypeExtensionNames
    {
        public const string CustomFields = "Custom Fields";
    }
}