using System;

namespace WarpCore.Platform.Kernel
{
    public static class Dependency
    {
        public static T Resolve<T>()
        {
            return (T)TinyIoCContainer.Current.Resolve(typeof(T));
        }

        public static void Register<T>(Type implementation) where T:class
        {
            TinyIoCContainer.Current.Register(typeof(T), implementation);
        }

        public static void Register<T>(Func<T> factory) where T : class
        {
            TinyIoCContainer.Current.Register<T>((x,y)=>factory());
        }
    }
}