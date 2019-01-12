using System;
using System.Linq;
using System.Reflection;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility.DynamicContent
{

    [Table("cms_content_type")]
    [WarpCoreEntity(ApiId, TitleProperty = nameof(ContentNamePlural), ContentNameSingular = "Content Type", SupportsCustomFields = false)]
    public class DynamicContentType : UnversionedContentEntity
    {
        public const string ApiId = "d90385d7-6f23-48a2-a4e4-fa1f75163724";

        public string Name { get; set; }
        public Guid TypeResolverId { get; set; }
        public string CustomAssemblyQualifiedTypeName { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string ContentNameSingular { get; set; }
        public string ContentNamePlural { get; set; }
        public bool SupportsCustomFields { get; set; }
        public string TitleProperty { get; set; }
    }

    [ExposeToWarpCoreApi(ApiId)]
    public class ContentTypeMetadataRepository : UnversionedContentRepository<DynamicContentType>
    {
        public const string ApiId = "99e051c8-602c-4fbf-adb2-79e464769387";

        public DynamicContentType GetContentType(Type clrType)
        {
            var attribute = clrType.GetCustomAttribute<WarpCoreEntityAttribute>();
            return this.Find().Single(x => x.TypeResolverId == attribute.TypeExtensionUid);
        }
    }
}
