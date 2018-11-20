using System;
using System.Web.Script.Serialization;

namespace WarpCore.Platform.Kernel
{
    public interface ISupportsJsonTypeConverter
    {

    }

    public static class ExtensibleTypeConverter
    {

        public static TChangeType ChangeType<TChangeType>(object value)
        {
            return (TChangeType)ChangeType(value, typeof(TChangeType));
        }

        public static object ChangeType(object value, Type type)
        {
            if (value == null && convertToType.IsGenericType) return Activator.CreateInstance(convertToType);
            if (value == null) return null;
            if (convertToType == value.GetType()) return value;
            if (convertToType.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(convertToType, value as string);
                else
                    return Enum.ToObject(convertToType, value);
            }
            if (!convertToType.IsInterface && convertToType.IsGenericType)
            {
                Type innerType = convertToType.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(convertToType, new object[] { innerValue });
            }
            if (value is string && convertToType == typeof(Guid)) return new Guid(value as string);
            if (value is string && convertToType == typeof(Version)) return new Version(value as string);
            if (value is string && convertToType == typeof(Type)) return Type.GetType(value as string);

            if (value is Type && convertToType == typeof(string)) return ((Type) value).AssemblyQualifiedName;


            if (value is string && typeof(ISupportsJsonTypeConverter).IsAssignableFrom(convertToType))
                return new JavaScriptSerializer().Deserialize((string)value,convertToType);

            if (value is ISupportsJsonTypeConverter && convertToType == typeof(string))
                return new JavaScriptSerializer().Serialize(value);

            if (convertToType == typeof(string))
                return value?.ToString();

            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, convertToType);
        }
    }
}
