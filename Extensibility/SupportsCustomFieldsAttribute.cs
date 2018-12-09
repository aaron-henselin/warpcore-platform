using System;
using System.Reflection;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility
{
    public class WarpCoreEntityAttribute : Attribute
    {
        public WarpCoreEntityAttribute(string typeExtensionId)
        {
            TypeExtensionUid = new Guid(typeExtensionId);
        }

        public bool SupportsCustomFields { get; set; } = true;
        public Guid TypeExtensionUid { get; set; }
        public string TitleProperty { get; set; }
        public string ContentNameSingular { get; set; }
        public string ContentNamePlural { get; set; }
    }

    public static class WarpCoreEntityExtensions
    {

        public static WarpCoreEntityAttribute GetEntityAttribute(this WarpCoreEntity entity)
        {
            var entityType = entity.GetType();
            var atr = (WarpCoreEntityAttribute)entityType.GetCustomAttribute(typeof(WarpCoreEntityAttribute));
            return atr;
        }


    }

}