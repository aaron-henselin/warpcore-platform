using System;
using System.Reflection;

namespace Framework
{
    public static class PropertyInfoExtensions
    {
        public static bool HasAttribute<T>(this PropertyInfo propertyInfo) where T :Attribute
        {
            return propertyInfo.GetCustomAttribute(typeof(T),true) != null;
        }
    }
}