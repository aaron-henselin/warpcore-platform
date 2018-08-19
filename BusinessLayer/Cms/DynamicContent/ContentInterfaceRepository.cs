using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.DynamicContent
{
   

    public class ContentInterfaceRepository : UnversionedContentRepository<ContentInterface>
    {
        public ContentInterface GetCustomFieldsTypeExtension(Guid uid)
        {
            return Find().Single(x => x.ContentTypeId == uid && x.InterfaceName == KnownTypeExtensionNames.CustomFields);
        }
    }

    [Table("cms_content_type_interface")]
    public class ContentInterface : UnversionedContentEntity
    {
        public Guid ContentTypeId { get; set; }

        public string InterfaceName { get; set; }

        public List<ContentField> InterfaceFields { get; set; } = new List<ContentField>();
    }


    public struct KnownTypeExtensionNames
    {
        public const string CustomFields = "Custom Fields";
    }
}