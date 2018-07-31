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
    [CosmosEntity(AllowCustomFields = true, Uid = "cms-toolbox-item")]
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
        public List<DynamicContentDefinition> DynamicContentDefinitions { get; set; } = new List<DynamicContentDefinition>();
    }

    public class DynamicContentDefinitionResolver : IDynamicContentDefinitionResolver
    {
        static Dictionary<Type,DynamicContentDefinition> _definitions = new Dictionary<Type, DynamicContentDefinition>();

        public DynamicContentDefinition Resolve(Type type)
        {
            if (_definitions.ContainsKey(type))
                return _definitions[type];


            var dynamiContentDefinitions = new RepositoryMetadataManager().Find().SelectMany(x => x.DynamicContentDefinitions);

            var cosmosEntityAttribute = type.GetCustomAttribute<TypeResolverKnownTypeAttribute>();
            if (cosmosEntityAttribute != null)
            {
                
                var def = dynamiContentDefinitions.SingleOrDefault(x => x.EntityUid == cosmosEntityAttribute.TypeUid);
                _definitions.Add(type, def);
                return _definitions[type];
            }

            var altDef = dynamiContentDefinitions.SingleOrDefault(x => x.EntityUid == type.AssemblyQualifiedName);
            _definitions.Add(type, altDef);
            return _definitions[type];
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
