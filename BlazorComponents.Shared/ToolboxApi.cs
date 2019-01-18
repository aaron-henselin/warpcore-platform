using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorComponents.Shared
{
    public class ToolboxViewModel
    {
        public List<ToolboxCategory> ToolboxCategories { get; set; } = new List<ToolboxCategory>();

    }

    public class ToolboxCategory
    {
        public string CategoryName { get; set; }
        public List<ToolboxItemViewModel> Items { get; set; } = new List<ToolboxItemViewModel>();
    }

    public class ToolboxItemViewModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string WidgetTypeCode { get; set; }
    }
}
