using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms.Toolbox
{


    [Table("cms_toolbox_item")]
    public class ToolboxItem : UnversionedContentEntity
    {
        public Guid SectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullyQualifiedTypeName { get; set; }
        public string AscxPath { get; set; }
        public string Category { get; set; }
    }

    public class ToolboxManager : UnversionedContentRepository<ToolboxItem>
    {
        public ToolboxItem GetToolboxItemByCode(string code)
        {
            return Orm.FindUnversionedContent<ToolboxItem>("Name eq '" + code + "'").Result.Single();
        }
    }
}
