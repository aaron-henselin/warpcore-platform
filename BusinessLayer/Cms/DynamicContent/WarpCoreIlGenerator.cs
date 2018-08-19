using System;
using System.Reflection;
using System.Reflection.Emit;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.DynamicContent
{
    public class WarpCoreIlGenerator
    {
        public static Type CreateDynamicRepository(Type entityType)
        {
            //var baseConstructor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[1] { typeof(Guid) }, null);

            var repoBaseType = typeof(VersionedContentRepository<>).MakeGenericType(entityType);


            // Create a Type Builder that generates a type directly into the current AppDomain.
            var appDomain = AppDomain.CurrentDomain;
            var assemblyName = new AssemblyName("Warpcore.DynamicTypes");
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var clrName = "DynamicContentRepository" + entityType.Name;
            var typeBuilder = moduleBuilder.DefineType(clrName, TypeAttributes.Class | TypeAttributes.Public, repoBaseType);

            // Create a parameterless (default) constructor.
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            return typeBuilder.CreateType();
        }

        public static Type CreateDynamicContentEntity(Guid dynamicTypeId)
        {
            var baseType = typeof(DynamicVersionedContent);
            var baseConstructor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[1] { typeof(Guid) }, null);

            // Create a Type Builder that generates a type directly into the current AppDomain.
            var appDomain = AppDomain.CurrentDomain;
            var assemblyName = new AssemblyName("Warpcore.DynamicTypes");
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

            var clrName = "DynamicContent" + dynamicTypeId.ToString().ToLower().Replace("-", "");
            var typeBuilder = moduleBuilder.DefineType(clrName, TypeAttributes.Class | TypeAttributes.Public, baseType);

            // Create a parameterless (default) constructor.
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);

            var ilGenerator = constructor.GetILGenerator();

            LocalBuilder locAi = ilGenerator.DeclareLocal(typeof(Guid));
            ilGenerator.Emit(OpCodes.Ldstr, dynamicTypeId.ToString());
            ilGenerator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new[] { typeof(string) }));

            ilGenerator.Emit(OpCodes.Stloc_0);

            // Generate constructor code
            ilGenerator.Emit(OpCodes.Ldarg_0);                // push &quot;this&quot; onto stack.
            ilGenerator.Emit(OpCodes.Ldloc_0);                // push guid onto stack.

            ilGenerator.Emit(OpCodes.Call, baseConstructor);  // call base constructor

            ilGenerator.Emit(OpCodes.Nop);                    // C# compiler add 2 NOPS, so
            ilGenerator.Emit(OpCodes.Nop);                    // we'll add them, too.

            ilGenerator.Emit(OpCodes.Ret);                    // Return

            return typeBuilder.CreateType();
        }

    }
}