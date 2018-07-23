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
        public string FullyQualifiedTypeName { get; set; }
        public string AscxPath { get; set; }
        public string Category { get; set; }

        public string FriendlyName { get; set; }
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

        public static Type ResolveToolboxItemType(ToolboxItem toolboxItem)
        {
            if (!string.IsNullOrWhiteSpace(toolboxItem.FullyQualifiedTypeName))
                return Type.GetType(toolboxItem.FullyQualifiedTypeName);

            return BuildManager.GetCompiledType(toolboxItem.AscxPath);
        }
    }
}
