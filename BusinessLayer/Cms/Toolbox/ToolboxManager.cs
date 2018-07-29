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
    [CosmosEntity(AllowCustomFields = false,Uid= "cms-repository-metadata")]
    public class RepositoryMetdata : UnversionedContentEntity
    {
        public string RepositoryUid { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string ContentName { get; set; }
        public List<DynamicContentDefinition> DynamicContentDefinitions { get; set; } = new List<DynamicContentDefinition>();
    }

    public class DynamicContentDefinitionResolver : IDynamicContentDefinitionResolver
    {
        static Dictionary<Type,DynamicContentDefinition> _definitions = new Dictionary<Type, DynamicContentDefinition>();

        public DynamicContentDefinition Resolve(Type type)
        {
            if (_definitions.ContainsKey(type))
                return _definitions[type];

            var cosmosEntityAttribute = type.GetCustomAttribute<CosmosEntityAttribute>();
            if (cosmosEntityAttribute == null)
                return null;

            if (!cosmosEntityAttribute.AllowCustomFields)
                return null;

            var dynamiContentDefinitions = new RepositoryMetadataManager().Find().SelectMany(x => x.DynamicContentDefinitions);
            var def = dynamiContentDefinitions.SingleOrDefault(x => x.EntityUid == cosmosEntityAttribute.Uid);
            _definitions.Add(type,def);
            return _definitions[type];
        }
    }


    [RepositoryUid("3f4d69f2-5849-4a49-9ee5-ad91b9ad251e", ManagedContentFriendlyName = "Repositories")]
    public class RepositoryMetadataManager : UnversionedContentRepository<RepositoryMetdata>
    {

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
