using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WarpCore.DbEngines.AzureStorage
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> HavingAttribute<T>(this IEnumerable<Type> types) where T : Attribute
        {
            return types.Where(x => x.GetCustomAttribute<T>() != null);
        }

        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

    }
}