using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Framework;

namespace Cms.Toolbox
{
    public class IsWarpCorePluginAssemblyAttribute : Attribute
    {
    }

    public class SettingAttribute : Attribute
    {
    }

    public class SettingProperty
    {
        public PropertyInfo PropertyInfo { get; set; }
        public string DisplayName { get; set; }
    }

    public class IncludeInToolboxAttribute : Attribute
    {
        public string WidgetUid { get; set; }

        public string FriendlyName { get; set; }
        public string Category { get; set; }
    }
    public class ToolboxMetadata
    {
        public string WidgetUid { get; set; }
        public string FriendlyName { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string Category { get; set; }
    }

    internal class AttributeBasedToolboxMetadataReader : IToolboxMetadataReader
    {

        public ToolboxMetadata ReadMetadata(Type type)
        {
            var includeInToolboxAtr = type.GetCustomAttribute<IncludeInToolboxAttribute>();
            if (includeInToolboxAtr == null)
                return null;

            return new ToolboxMetadata
            {
                WidgetUid = includeInToolboxAtr.WidgetUid,
                FriendlyName = includeInToolboxAtr.FriendlyName,
                AssemblyQualifiedTypeName = type.AssemblyQualifiedName,
                Category = includeInToolboxAtr.Category
            };
        }
    }

    public interface IToolboxMetadataReader
    {
        ToolboxMetadata ReadMetadata(Type type);
    }

    public static class ToolboxPropertyFilter
    {
        public static Func<PropertyInfo, bool> IsConfigurable => x => x.HasAttribute<SettingAttribute>();
    }

    public static class RepositoryMetadataReader
    {
        public static void X()
        {
            
        }
    }

    public static class ToolboxMetadataReader
    {
        public static ToolboxMetadata ReadMetadata(Type type)
        {
            var atr = new AttributeBasedToolboxMetadataReader();
            return atr.ReadMetadata(type);
        }
        public static IReadOnlyCollection<SettingProperty> ReadProperties(Type clrType, Func<PropertyInfo,bool> propertyFilter)
        {
            List<SettingProperty> properties = new List<SettingProperty>();
            foreach (var property in clrType.GetProperties())
            {
                var include = propertyFilter(property);
                if (!include)
                    continue;


                var displayNameDefinition = (DisplayNameAttribute)property.GetCustomAttribute(typeof(DisplayNameAttribute));
                properties.Add(new SettingProperty
                {
                    PropertyInfo = property,
                    DisplayName = displayNameDefinition?.DisplayName ?? property.Name,

                });
            }

            return properties;
        }

    }

}
