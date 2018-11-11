using System;
using System.ComponentModel.DataAnnotations.Schema;
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
    }

    public class ContentTypeMetadataRepository : UnversionedContentRepository<DynamicContentType>
    {
    }
}
