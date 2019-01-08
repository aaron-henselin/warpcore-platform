using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WarpCore.Web.EmbeddedResourceVirtualPathProvider
{
    public static class EmbeddedResourcePathFactory
    {
        public static string CreateVppAsmDirectoryPathForViews(Assembly asm)
        {
            var asmName = asm.FullName;
            foreach (var character in Path.GetInvalidFileNameChars())
                asmName = asmName.Replace(character, '_');
            
            return $"/Views/{asmName}";
        }

        public static string CreateMvcViewLocatorPath(Type type)
        {

            var asmDir = WarpCore.Web.EmbeddedResourceVirtualPathProvider.EmbeddedResourcePathFactory.CreateVppAsmDirectoryPathForViews(type.Assembly);

            var shortName = type.Namespace.Substring(type.Assembly.GetName().Name.Length + 1);
            if (shortName.Length == 0)
                return asmDir+"/";

            if (!shortName.Contains("."))
                return asmDir + "/";

            var lastNamespaceMarker = shortName.LastIndexOf(".");
            return asmDir + "/" + shortName.Substring(0, lastNamespaceMarker)+".";
        }
    }
}