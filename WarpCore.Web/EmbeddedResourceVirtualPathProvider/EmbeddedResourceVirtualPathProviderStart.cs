using System.Linq;
using System.Reflection;

namespace WarpCore.Web.EmbeddedResourceVirtualPathProvider
{
    public static class EmbeddedResourceVirtualPathProviderStart
    {
        public static void Start()
        {
            //By default, we scan all non system assemblies for embedded resources
            var assemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies()
                .Cast<Assembly>()
                .Where(a => a.GetName().Name.StartsWith("System") == false);

            var vpp = new Vpp();
            foreach (var asm in assemblies)
                vpp.Add(asm);

            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(vpp);
        }
    }
}