using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.DynamicContent
{
    [FormDesignerInterop(FormDesignerUid)]
    public class DynamicRepository
    {
        public const string FormDesignerUid = "cdeb5593-a6b5-4b1f-9a0c-ab32047eac90";

    }

    [Table("cms_type_extension")]
    public class TypeExtension : UnversionedContentEntity
    {
        public Guid TypeResolverUid { get; set; }

        public string ExtensionName { get; set; }

        public List<DynamicPropertyDescription> DynamicProperties { get; set; } =
            new List<DynamicPropertyDescription>();
    }

    public class DynamicTypeDefinitionResolver : IDynamicTypeDefinitionResolver
    {
        static Dictionary<Guid, DynamicTypeDefinition> _definitions = new Dictionary<Guid, DynamicTypeDefinition>();

        public DynamicTypeDefinition Resolve(Guid uid)
        {
            if (_definitions.ContainsKey(uid))
                return _definitions[uid];

            var typeExtensions = new TypeExtensionRepository().Find().Where(x => x.TypeResolverUid == uid);

            var dtd = new DynamicTypeDefinition();
            foreach (var extension in typeExtensions)
            {
                dtd.DynamicProperties.AddRange(extension.DynamicProperties);
            }

            _definitions.Add(uid, dtd);
            return _definitions[uid];
        }

        public DynamicTypeDefinition Resolve(Type type)
        {
            var cosmosEntityAttribute = type.GetCustomAttribute<SupportsCustomFieldsAttribute>();
            if (cosmosEntityAttribute != null)
                return Resolve(cosmosEntityAttribute.TypeExtensionUid);

            return null;
        }
    }

    public struct KnownTypeExtensionNames
    {
        public const string CustomFields = "CustomFields";
    }

    public class TypeExtensionRepository : UnversionedContentRepository<TypeExtension>
    {
        public TypeExtension GetCustomFieldsTypeExtension(Guid uid)
        {
            return Find().Single(x => x.TypeResolverUid == uid && x.ExtensionName == KnownTypeExtensionNames.CustomFields);
        }
    }

}
