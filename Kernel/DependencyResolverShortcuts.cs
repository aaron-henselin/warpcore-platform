using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.Platform.Kernel
{
    public static class Dependency
    {
        public static T Resolve<T>()
        {
            return (T)TinyIoCContainer.Current.Resolve(typeof(T));
        }

        public static IEnumerable<T> ResolveMultiple<T>()
        {
            return (IEnumerable<T>)TinyIoCContainer.Current.Resolve(typeof(IEnumerable<T>));
        }

        public static void RegisterMultiple<T>(IEnumerable<Type> implementations) where T : class
        {
            TinyIoCContainer.Current.RegisterMultiple(typeof(T), implementations);
        }

        public static void RegisterAll<T>()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();
            var result = allTypes.Where(x => typeof(T).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();

            TinyIoCContainer.Current.RegisterMultiple<T>(result);
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