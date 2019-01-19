using System;
using System.Collections.Generic;
using System.Reflection;
using Cms_PageDesigner_Context;
using WarpCore.Platform.Kernel;

namespace WarpCore.Web.Widgets.FormBuilder.Support
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



    public class ConfiguratorEvents
    {
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public void RaiseValueChanged(ValueChangedEventArgs valueChangedEvent)
        {
            ValueChanged?.Invoke(this,valueChangedEvent);
        }
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public IDictionary<string, string> Model { get; set; }
    }
}