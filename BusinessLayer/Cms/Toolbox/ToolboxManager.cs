using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WarpCore.Cms.Toolbox
{
    [Table("cms_toolbox_section")]
    public class ToolboxSection
    {
        public string Name { get; set; }
        
    }

    [Table("cms_toolbox_item")]
    public class ToolboxItem
    {
        public Guid SectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullyQualifiedTypeName { get; set; }
        public string AscxPath { get; set; }
    }

    public class ToolboxManager
    {
        public ToolboxItem GetToolboxItemByCode(string code)
        {
            return new ToolboxItem();
        }
    }
}
