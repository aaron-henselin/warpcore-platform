using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Framework
{
    public interface ISupportsJsonTypeConverter
    {

    }

    public static class ExtensibleTypeConverter
    {
        public static object ChangeType(object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }
            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);

            if (value is string && typeof(ISupportsJsonTypeConverter).IsAssignableFrom(type))
                return new JavaScriptSerializer().Deserialize((string)value,type);

            if (value is ISupportsJsonTypeConverter && type == typeof(string))
                return new JavaScriptSerializer().Serialize(value);

            if (type == typeof(string))
                return value?.ToString();

            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, type);
        }
    }
}
