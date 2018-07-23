using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cms.Toolbox
{
    public class SettingAttribute : Attribute
    {
    }


    public class IncludeInToolboxAttribute : Attribute
    {
        public string WidgetUid { get; set; }

        public string FriendlyName { get; set; }
    }
    public class ToolboxMetadata
    {
        public string WidgetUid { get; set; }
        public string FriendlyName { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
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
                AssemblyQualifiedTypeName = type.AssemblyQualifiedName
            };
        }
    }

    public interface IToolboxMetadataReader
    {
        ToolboxMetadata ReadMetadata(Type type);
    }

    public static class ToolboxMetadataReader
    {
        public static ToolboxMetadata ReadMetadata(Type type)
        {
            var atr = new AttributeBasedToolboxMetadataReader();
            return atr.ReadMetadata(type);
        }
    }

}
