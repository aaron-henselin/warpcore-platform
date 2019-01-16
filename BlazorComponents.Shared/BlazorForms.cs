using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorComponents.Shared
{
    public class BlazorForm
    {
        public BlazorFormNode RootNode { get; set; }
    }

    public class BlazorFormNode
    {
        public string ControlName { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, List<BlazorFormNode>> ChildContent { get; set; } = new Dictionary<string, List<BlazorFormNode>>();
    }


}
