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

        public Guid TypeExtensionUid { get; set; }
        public string Title { get; set; }
    }

    public static class WarpCoreEntityExtensions
    {
        public static string GetTitle(this WarpCoreEntity entity)
        {
            var entityType = entity.GetType();
            var atr = (WarpCoreEntityAttribute)entityType.GetCustomAttribute(typeof(WarpCoreEntityAttribute));
            if (atr?.Title == null)
                return entity.ContentId.ToString();

            var val = entityType.GetProperty(atr.Title).GetValue(entity)?.ToString();
            return val;
        }
    }

}