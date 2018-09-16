using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using Cms.DynamicContent;
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
        public string FormInteropUid { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string CustomAssemblyQualifiedTypeName { get; set; }
        public string FriendlyRepositoryName { get; set; }
        public bool IsDynamic { get; set; }
    }



    public class RepositoryMetadataManager : UnversionedContentRepository<RepositoryMetdata>
    {
        public RepositoryMetdata GetRepositoryMetdataByTypeResolverUid(Guid formInteropUid)
        {
            return Find(nameof(RepositoryMetdata.FormInteropUid) + " eq '" + formInteropUid + "'").First();
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
            if (!string.IsNullOrWhiteSpace(toolboxItem.AscxPath))
                return BuildManager.GetCompiledType(toolboxItem.AscxPath);

            return Type.GetType(toolboxItem.AssemblyQualifiedTypeName);
        }
    }
}
