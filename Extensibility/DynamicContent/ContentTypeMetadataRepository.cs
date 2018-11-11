using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.DynamicContent
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
