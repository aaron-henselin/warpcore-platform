using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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

    [Table("cms_toolbox_item")]
    public class RepositoryMetdata : UnversionedContentEntity
    {
        public string RepositoryUid { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
    }
    
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
