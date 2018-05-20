using System;
using System.Collections.Generic;
using System.Text;

namespace WarpCore.Cms.Toolbox
{
    public class ToolboxItem
    {
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
