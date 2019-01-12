using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WarpCore.Platform.Kernel
{
    public static class AssemblyLoader
    {
        public static void LoadAssemblies(AppDomain appDomain, Func<Assembly, bool> loadCondition)
        {
            //todo: add private search relative path.
            var binariesToCheck = Directory.GetFiles(appDomain.BaseDirectory, "*.dll").ToList();

            var alreadyLoaded = appDomain
                .GetAssemblies()
                .Where(x => !x.IsDynamic)
                .Select(x => x.Location);

            binariesToCheck = binariesToCheck.Except(alreadyLoaded).ToList();

            foreach (var file in binariesToCheck)
            {
                Assembly.LoadFile(file);
            }
        }
    }
}