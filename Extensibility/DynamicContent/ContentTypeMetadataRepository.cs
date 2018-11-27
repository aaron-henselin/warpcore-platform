using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility.DynamicContent
{
    [Table("cms_content_type")]
    public class DynamicContentType : UnversionedContentEntity
    {
        public string Name { get; set; }
        public Guid TypeResolverId { get; set; }
        public string CustomAssemblyQualifiedTypeName { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string ContentNameSingular { get; set; }
        public string ContentNamePlural { get; set; }
    }

    public class ContentTypeMetadataRepository : UnversionedContentRepository<DynamicContentType>
    {
        public DynamicContentType GetContentType(Type clrType)
        {
            var attribute = clrType.GetCustomAttribute<WarpCoreEntityAttribute>();
            return this.Find().Single(x => x.TypeResolverId == attribute.TypeExtensionUid);
        }
    }
}
