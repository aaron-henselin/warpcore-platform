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
            
            var binariesToCheck = Directory.GetFiles(appDomain.RelativeSearchPath, "*.dll").ToList();

            var alreadyLoaded = appDomain
                .GetAssemblies()
                .ToDictionary(x => x.FullName);
         

            foreach (var file in binariesToCheck)
            {
                var asmName = AssemblyName.GetAssemblyName(file);
                var fullName = asmName.FullName;

                if (alreadyLoaded.ContainsKey(fullName))
                    continue;

                var bytes = File.ReadAllBytes(file);
                Assembly.Load(bytes);
            }
        }
    }
}