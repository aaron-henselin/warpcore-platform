using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Platform.Orm;

namespace Cms.Toolbox
{




    public class ListOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public enum SettingType { Text,OptionList, CheckBox}

    public class SettingAttribute : Attribute
    {
        public SettingType SettingType { get; set; }

        public Type ConfiguratorType { get; set; }
        
    }

    public class SettingProperty
    {
        public PropertyInfo PropertyInfo { get; set; }
        public string DisplayName { get; set; }
        public SettingType? SettingType { get; set; }
        public Type ConfiguratorType { get; set; }
    }

    public class IncludeInToolboxAttribute : Attribute
    {
        public string WidgetUid { get; set; }

        public string FriendlyName { get; set; }
        public string Category { get; set; }
        public string AscxPath { get; set; }
    }
    public class ToolboxMetadata
    {
        public string WidgetUid { get; set; }
        public string FriendlyName { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string Category { get; set; }
        public string AscxPath { get; set; }
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
                Category = includeInToolboxAtr.Category,
                AscxPath = includeInToolboxAtr.AscxPath
            };
        }
    }

    public interface IToolboxMetadataReader
    {
        ToolboxMetadata ReadMetadata(Type type);
    }

    public static class ToolboxPropertyFilter
    {
        public static Func<PropertyInfo, bool> IsSettingProperty => x => x.HasAttribute<SettingAttribute>() && IsNotIgnoredType(x);
        public static Func<PropertyInfo, bool> IsNotIgnoredType => x => x.DeclaringType != typeof(Control) &&
                                                                        x.DeclaringType != typeof(WarpCoreEntity) &&
                                                                        x.DeclaringType != typeof(VersionedContentEntity) &&
                                                                        x.DeclaringType != typeof(UnversionedContentEntity);

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

                var settingInfo = property.GetCustomAttributes().OfType<SettingAttribute>().FirstOrDefault();
                var displayNameDefinition = (DisplayNameAttribute)property.GetCustomAttribute(typeof(DisplayNameAttribute));
                properties.Add(new SettingProperty
                {
                    PropertyInfo = property,
                    DisplayName = displayNameDefinition?.DisplayName ?? property.Name,
                    ConfiguratorType = settingInfo?.ConfiguratorType,
                    SettingType = settingInfo?.SettingType
                });
            }

            return properties;
        }

    }

}
