using TinyIoC;

namespace WarpCore.DbEngines.AzureStorage
{
    public static class Dependency
    {
        public static T Resolve<T>()
        {
            return (T)TinyIoCContainer.Current.Resolve(typeof(T));
        }
    }
}