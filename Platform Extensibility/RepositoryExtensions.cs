using System.Reflection;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility
{
    public static class RepositoryExtensions
    {
        public static ExposeToWarpCoreApi GetRepositoryAttribute(this ISupportsCmsForms entity)
        {
            var entityType = entity.GetType();
            var atr = (ExposeToWarpCoreApi)entityType.GetCustomAttribute(typeof(ExposeToWarpCoreApi));
            return atr;
        }
    }
}