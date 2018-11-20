using System;
using System.Collections.Generic;
using System.Reflection;
using WarpCore.Platform.Kernel;

namespace WarpCore.Web.Widgets.FormBuilder
{
    public static class IDictionaryExtensions
    {
        public static TType Get<TType>(this IDictionary<string, string> dictionary, string key)
        {
            var val = dictionary[key];
            return ExtensibleTypeConverter.ChangeType<TType>(val);
        }

        public static void Set<TType>(this IDictionary<string, string> dictionary, string key, TType value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key,null);

            dictionary[key] = ExtensibleTypeConverter.ChangeType<string>(value);
        }
    }

    public class ConfiguratorEditingContext
    {
        public IDictionary<string, string> CurrentValues { get; set; }
        public Type ClrType { get; set; }
        public Func<PropertyInfo, bool> PropertyFilter { get; set; }
        public EditingContext ParentEditingContext { get; set; }
    }
}