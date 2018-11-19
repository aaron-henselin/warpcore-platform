using System;
using System.Collections.Generic;
using System.Reflection;

namespace WarpCore.Web.Widgets.FormBuilder.Support
{
    public class ConfiguratorEditingContext
    {
        public IDictionary<string, string> CurrentValues { get; set; }
        public Type ClrType { get; set; }
        public Func<PropertyInfo, bool> PropertyFilter { get; set; }
        public EditingContext ParentEditingContext { get; set; }
    }
}