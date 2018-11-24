﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WarpCore.Platform.Kernel.Extensions
{
    public static class TypeExtensions
    {
        public static string GetDisplayName(this Type type)
        {
            return type.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                              ?? type.Name;
        }

        public static IEnumerable<PropertyInfo> GetPropertiesFiltered(this Type type, Func<PropertyInfo, bool> condition)
        {
            foreach (var property in type.GetProperties())
            {
                if (condition(property))
                    yield return property;
            }
        }

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