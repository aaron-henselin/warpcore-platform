using System;
using System.Reflection;

namespace WarpCore.Platform.Kernel.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static bool HasAttribute<T>(this PropertyInfo propertyInfo) where T :Attribute
        {
            return propertyInfo.GetCustomAttribute(typeof(T),true) != null;
        }
    }
}