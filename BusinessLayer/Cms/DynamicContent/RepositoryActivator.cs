using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Framework;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.DynamicContent
{
    
    public static class RepositoryTypeResolver
    {
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();


        public static Type ResolveDynamicTypeByInteropId(Guid id)
        {
            if (!_types.ContainsKey(id))
            {
                var entityType = DynamicTypeBuilder.TypeBuilderHelper.CreateDynamicContentEntity(id);
                Activator.CreateInstance(entityType);
                var repoType = DynamicTypeBuilder.TypeBuilderHelper.CreateDynamicRepository(entityType);
                Activator.CreateInstance(repoType);
                _types.Add(id,repoType);
            }

            return _types[id];
        }
    }
    
    public class DynamicTypeBuilder
    {
        public static class TypeBuilderHelper
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
                var baseConstructor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[1]{typeof(Guid)}, null);

                // Create a Type Builder that generates a type directly into the current AppDomain.
                var appDomain = AppDomain.CurrentDomain;
                var assemblyName = new AssemblyName("Warpcore.DynamicTypes");
                var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

                var clrName = "DynamicContent" + dynamicTypeId.ToString().ToLower().Replace("-","");
                var typeBuilder = moduleBuilder.DefineType(clrName, TypeAttributes.Class | TypeAttributes.Public, baseType);

                // Create a parameterless (default) constructor.
                var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);

                var ilGenerator = constructor.GetILGenerator();
         
                LocalBuilder locAi = ilGenerator.DeclareLocal(typeof(Guid));
                ilGenerator.Emit(OpCodes.Ldstr,dynamicTypeId.ToString());
                ilGenerator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new []{typeof(string)}));

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


    [FormDesignerInterop(FormDesignerUid)]
    public class DynamicUnversionedContentRepository
    {
        public const string FormDesignerUid = "8cc6a6d4-b6e9-4693-b94f-1d7868fe781c";

    }


    [Table("cms_type_extension")]
    public class TypeExtension : UnversionedContentEntity
    {
        public Guid TypeResolverUid { get; set; }

        public string ExtensionName { get; set; }

        public List<DynamicPropertyDescription> DynamicProperties { get; set; } =
            new List<DynamicPropertyDescription>();
    }

    public class DynamicTypeDefinitionResolver : IDynamicTypeDefinitionResolver
    {
        static Dictionary<Guid, DynamicTypeDefinition> _definitions = new Dictionary<Guid, DynamicTypeDefinition>();

        public DynamicTypeDefinition Resolve(Guid uid)
        {
            if (_definitions.ContainsKey(uid))
                return _definitions[uid];

            var typeExtensions = new TypeExtensionRepository().Find().Where(x => x.TypeResolverUid == uid);

            var dtd = new DynamicTypeDefinition();
            foreach (var extension in typeExtensions)
            {
                dtd.DynamicProperties.AddRange(extension.DynamicProperties);
            }

            _definitions.Add(uid, dtd);
            return _definitions[uid];
        }

        public DynamicTypeDefinition Resolve(Type type)
        {
            var cosmosEntityAttribute = type.GetCustomAttribute<SupportsCustomFieldsAttribute>();
            if (cosmosEntityAttribute != null)
                return Resolve(cosmosEntityAttribute.TypeExtensionUid);

            return null;
        }
    }

    public struct KnownTypeExtensionNames
    {
        public const string CustomFields = "CustomFields";
    }

    public class TypeExtensionRepository : UnversionedContentRepository<TypeExtension>
    {
        public TypeExtension GetCustomFieldsTypeExtension(Guid uid)
        {
            return Find().Single(x => x.TypeResolverUid == uid && x.ExtensionName == KnownTypeExtensionNames.CustomFields);
        }
    }

}
