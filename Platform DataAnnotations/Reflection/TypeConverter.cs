using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using WarpCore.Platform.DataAnnotations;

namespace WarpCore.Platform.Kernel
{
    public interface IModuleInitializer
    {
        void InitializeModule();
    }


    public interface ITypeConverterExtension
    { 
        bool TryChangeType(object value, Type toType, out object newValue);
    }

    public class EnumTypeConverter : ITypeConverterExtension
    {
        public bool TryChangeType(object value, Type toType, out object newValue)
        {
            newValue = null;

            if (!toType.IsEnum)
                return false;

            if (value is string)
                newValue = Enum.Parse(toType, value as string);
            else
                newValue = Enum.ToObject(toType, value);

            return true;

        }
    }

    //public class JavascriptSerializerTypeConverter : ITypeConverterExtension
    //{

    //    public bool TryChangeType(object value, Type toType, out object newValue)
    //    {
    //        newValue = null;
    //        if (value is string && typeof(ISupportsJavaScriptSerializer).IsAssignableFrom(toType))
    //        {
    //            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes((string)value)))
    //            {
    //                // Deserialization from JSON  
    //                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(toType);
    //                newValue = deserializer.ReadObject(ms);
    //            }

    //            return true;
    //        }

    //        if (value is ISupportsJavaScriptSerializer && toType == typeof(string))
    //        {

    //            using (MemoryStream ms = new MemoryStream())
    //            {
    //                DataContractJsonSerializer serializer = new DataContractJsonSerializer(toType);
    //                serializer.WriteObject(ms, value);
    //                newValue = Encoding.Default.GetString(ms.ToArray());
    //            }
    //            return true;
    //        }

    //        return false;
    //    }
    //}

    public class JavascriptSerializerTypeConverter : ITypeConverterExtension
    {

        public bool TryChangeType(object value, Type toType, out object newValue)
        {
            newValue = null;
            if (value is string && typeof(ISupportsJavaScriptSerializer).IsAssignableFrom(toType))
            {
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes((string)value)))
                {
                    // Deserialization from JSON  
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(toType);
                    newValue = deserializer.ReadObject(ms);
                }

                return true;
            }

            if (value is ISupportsJavaScriptSerializer && toType == typeof(string))
            {

                using (MemoryStream ms = new MemoryStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(value.GetType());
                    serializer.WriteObject(ms, value);
                    newValue = Encoding.Default.GetString(ms.ToArray());
                }
                return true;
            }

            return false;
        }
    }

    public class GuidTypeConverter : ITypeConverterExtension
    {
        public bool TryChangeType(object value, Type toType, out object newValue)
        {
            newValue = default(Guid);
            if (value is string && toType == typeof(Guid))
            {
                newValue = new Guid(value as string);
                return true;
            }
            return false;
        }
    }

    public class TypeTypeConverter : ITypeConverterExtension
    {
        public bool TryChangeType(object value, Type toType, out object newValue)
        {
            newValue = null;
            if (value is string && toType == typeof(Type))
            {
                newValue = Type.GetType(value as string);
                return true;
            }

            if (value is Type && toType == typeof(string))
            {
                newValue = ((Type) value).AssemblyQualifiedName;
                return true;
            }

            return false;
        }
    }

    public static class ExtensibleTypeConverter
    {
        public static List<ITypeConverterExtension> TypeConverters { get; } = new List<ITypeConverterExtension>
        {
            new GuidTypeConverter(),
            new EnumTypeConverter(),
            new JavascriptSerializerTypeConverter(),
            new TypeTypeConverter()
        };

        public static TChangeType ChangeType<TChangeType>(object value)
        {
            return (TChangeType)ChangeType(value, typeof(TChangeType));
        }

        public static object ChangeType(object value, Type convertToType)
        {
            if (value == null && convertToType.IsGenericType) return Activator.CreateInstance(convertToType);
            if (value == null) return null;
            if (convertToType == value.GetType()) return value;

            foreach (var typeConverter in TypeConverters)
            {
                var success = typeConverter.TryChangeType(value, convertToType, out var newValue);
                if (success)
                    return newValue;

            }


            if (!convertToType.IsInterface && convertToType.IsGenericType)
            {
                Type innerType = convertToType.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(convertToType, new object[] { innerValue });
            }
            


            if (value is Uri && convertToType == typeof(string))
                return value.ToString();

            if (value is string && convertToType == typeof(Uri))
            {
                var baseUri = new Uri(value as string, UriKind.RelativeOrAbsolute);
                return baseUri;
            }



            if (convertToType == typeof(string))
                return value?.ToString();

            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, convertToType);
        }
    }
}
