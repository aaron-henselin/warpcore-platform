using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, string> GetPropertyValues(this object activated, Func<PropertyInfo,bool> condition)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var property in activated.GetType().GetProperties())
            {
                if (!property.CanRead)
                    continue;

                //var isSetting = property.GetCustomAttribute<SettingAttribute>() != null;
                //if (!isSetting)
                //    continue;

                var key = property.Name;
                var val = (string)DesignerTypeConverter.ChangeType(property.GetValue(activated), typeof(string));
                parameters.Add(key, val);
            }
            return parameters;
        }

        public static void SetPropertyValues(this object obj, IDictionary<string,string> valuesToSet, Func<PropertyInfo,bool> condition)
        {
            foreach (var kvp in valuesToSet)
            {
                var propertyInfo = obj.GetType().GetProperty(kvp.Key);
                if (propertyInfo == null || !propertyInfo.CanWrite || !condition(propertyInfo))
                    continue;

                SetPropertyValue(obj,propertyInfo,kvp.Value);
            }

        }

        public static void SetPropertyValue(this object obj, PropertyInfo propertyInfo, object valueToSet)
        {
            try
            {
                var newType = DesignerTypeConverter.ChangeType(valueToSet, propertyInfo.PropertyType);
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(obj, newType);
            }
            catch (Exception e)
            {

            }
        }
    }
}