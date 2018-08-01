using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms.Toolbox
{


    [Table("cms_toolbox_item")]
    public class ToolboxItem : UnversionedContentEntity
    {
        public string WidgetUid { get; set; }
        public string Description { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string AscxPath { get; set; }
        public string Category { get; set; }

        public string FriendlyName { get; set; }
    }



    [Table("cms_repository_metadata")]
    public class RepositoryMetdata : UnversionedContentEntity
    {
        public string TypeResolverUid { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }

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

    public class RepositoryMetadataManager : UnversionedContentRepository<RepositoryMetdata>
    {
        public RepositoryMetdata GetRepositoryMetdataByTypeResolverUid(Guid typeResolverUid)
        {
            return Find(nameof(RepositoryMetdata.TypeResolverUid) + " eq '" + typeResolverUid + "'").First();
        }
    }

    public class ToolboxManager : UnversionedContentRepository<ToolboxItem>
    {
        public ToolboxItem GetToolboxItemByCode(string code)
        {
            var toolboxResult = Orm.FindUnversionedContent<ToolboxItem>("WidgetUid eq '" + code + "'").Result;
            if (!toolboxResult.Any())
                throw new Exception($"Toolbox does not contain item '{code}'");

            return toolboxResult.Single();
        }

        public static Type ResolveToolboxItemClrType(ToolboxItem toolboxItem)
        {
            if (!string.IsNullOrWhiteSpace(toolboxItem.AssemblyQualifiedTypeName))
                return Type.GetType(toolboxItem.AssemblyQualifiedTypeName);

            return BuildManager.GetCompiledType(toolboxItem.AscxPath);
        }
    }
}
