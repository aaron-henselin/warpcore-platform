using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarpCore.Platform.Kernel.Extensions;

namespace WarpCore.Platform.Kernel
{


    public static class ObjectExtensions
    {



        public static IDictionary<string, string> GetPropertyValues(this object activated, Func<PropertyInfo,bool> condition)
        {
            var propertiesFilered = activated.GetType().GetPropertiesFiltered(condition).ToList();

            var parameters = new Dictionary<string, string>();
            foreach (var property in propertiesFilered)
            {
                if (!property.CanRead)
                    continue;
                
                var key = property.Name;
                var val = (string)ExtensibleTypeConverter.ChangeType(property.GetValue(activated), typeof(string));
                parameters.Add(key, val);
            }
            return parameters;
        }

        public static void SetPropertyValues(this object obj, IDictionary<string,string> valuesToSet, Func<PropertyInfo,bool> condition)
        {
            if (valuesToSet == null)
                return;

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
                var newType = ExtensibleTypeConverter.ChangeType(valueToSet, propertyInfo.PropertyType);
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(obj, newType);
            }
            catch (Exception e)
            {

            }
        }
    }
}