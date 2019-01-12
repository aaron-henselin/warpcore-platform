using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Orm;

namespace Cms.DynamicContent
{
    
    //public static class RepositoryTypeResolver
    //{
    //    private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();


    //    public static Type ResolveDynamicTypeByInteropId(Guid id)
    //    {
    //        if (!_types.ContainsKey(id))
    //        {
    //            var metadataManager = new RepositoryMetadataManager();
    //            var repositoryMetadata = metadataManager.GetRepositoryMetdataByTypeResolverUid(id);
    //            if (repositoryMetadata.IsDynamic)
    //            {
    //                var entityType = DynamicTypeBuilder.TypeBuilderHelper.CreateDynamicContentEntity(id);
    //                var repoType = DynamicTypeBuilder.TypeBuilderHelper.CreateDynamicRepository(entityType);
    //                _types.Add(id, repoType);
    //            }
    //            else
    //            {
    //                Type repoType = null;
    //                var metadata = metadataManager.GetRepositoryMetdataByTypeResolverUid(id);
    //                if (metadata.CustomAssemblyQualifiedTypeName != null)
    //                    repoType= Type.GetType(metadata.CustomAssemblyQualifiedTypeName);

    //                if (metadata.AssemblyQualifiedTypeName != null)
    //                    repoType= Type.GetType(metadata.AssemblyQualifiedTypeName);

    //                _types.Add(id, repoType);
    //            }

                
    //        }

    //        return _types[id];
    //    }
    //}
    
    public class DynamicTypeBuilder
    {
        public const string DynamicTypeAssembly = "Warpcore.DynamicTypes";

        public static class TypeBuilderHelper
        {
            

            public static Type CreateDynamicRepository(Type entityType)
            {
                //var baseConstructor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[1] { typeof(Guid) }, null);

                var repoBaseType = typeof(VersionedContentRepository<>).MakeGenericType(entityType);


                // Create a Type Builder that generates a type directly into the current AppDomain.
                var appDomain = AppDomain.CurrentDomain;
                var assemblyName = new AssemblyName(DynamicTypeAssembly);
                var assemblyBuilder =
                    AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.FullName);

                var clrName = "DynamicContentRepository" + entityType.Name;
                var typeBuilder = moduleBuilder.DefineType(clrName, TypeAttributes.Class | TypeAttributes.Public, repoBaseType);

                // Create a parameterless (default) constructor.
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
                return typeBuilder.CreateTypeInfo().AsType();
            }

            public static Type CreateDynamicContentEntity(Guid dynamicTypeId)
            {
                var baseType = typeof(DynamicVersionedContent);
                var baseConstructor = baseType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null, new Type[1]{typeof(Guid)}, null);

                // Create a Type Builder that generates a type directly into the current AppDomain.
                var appDomain = AppDomain.CurrentDomain;
                var assemblyName = new AssemblyName(DynamicTypeAssembly);
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);

                var clrName = "DynamicContent" + dynamicTypeId.ToString().ToLower().Replace("-","");
                var typeBuilder = moduleBuilder.DefineType(clrName, TypeAttributes.Class | TypeAttributes.Public, baseType);

                // Create a parameterless (default) constructor.
                var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
                var ilGenerator = constructor.GetILGenerator();
         
                LocalBuilder locAi = ilGenerator.DeclareLocal(typeof(Guid));                                //var g;
                ilGenerator.Emit(OpCodes.Ldstr,dynamicTypeId.ToString());                                   //var dynamicTypeId = '1234567567567'
                ilGenerator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new []{typeof(string)}));      //var newGuid = new Guid(dynamicTypeId)
                ilGenerator.Emit(OpCodes.Stloc_0);                                                          //g = newGuid;      

                // Generate constructor code
                ilGenerator.Emit(OpCodes.Ldarg_0);                                                          // var cotr_arg0 = this;
                ilGenerator.Emit(OpCodes.Ldloc_0);                                                          // var cotr_arg1 = g;

                ilGenerator.Emit(OpCodes.Call, baseConstructor);                                            // cotr_arg0.base(cotr_arg1);

                ilGenerator.Emit(OpCodes.Ret);                                                              // return

                return typeBuilder.CreateTypeInfo().AsType();
            }
        }

    }


    [ExposeToWarpCoreApi(FormDesignerUid)]
    public class DynamicUnversionedContentRepository
    {
        public const string FormDesignerUid = "8cc6a6d4-b6e9-4693-b94f-1d7868fe781c";

    }





    //public class DynamicTypeDefinitionResolver : IDynamicTypeDefinitionResolver
    //{
    //    static Dictionary<Guid, DynamicTypeDefinition> _definitions = new Dictionary<Guid, DynamicTypeDefinition>();

    //    public DynamicTypeDefinition Resolve(Guid uid)
    //    {
    //        if (_definitions.ContainsKey(uid))
    //            return _definitions[uid];

    //        var typeExtensions = new ContentInterfaceRepository().Find().Where(x => x.TypeResolverUid == uid);

    //        var dtd = new DynamicTypeDefinition();
    //        foreach (var extension in typeExtensions)
    //        {
    //            dtd.DynamicProperties.AddRange(extension.DynamicProperties);
    //        }

    //        _definitions.Add(uid, dtd);
    //        return _definitions[uid];
    //    }

    //    public DynamicTypeDefinition Resolve(Type type)
    //    {
    //        var cosmosEntityAttribute = type.GetCustomAttribute<ExposeToWarpCoreApi>();
    //        if (cosmosEntityAttribute != null)
    //            return Resolve(cosmosEntityAttribute.TypeUid);

    //        return null;
    //    }
    //}

    //public struct KnownTypeExtensionNames
    //{
    //    public const string CustomFields = "CustomFields";
    //}

    //public class FriendlyTypeInfo : UnversionedContentEntity
    //{
    //    public string FriendlyName { get; set; }

    //}

    //public class FiendlyTypeInfoRepository : UnversionedContentRepository<FriendlyTypeInfo>
    //{

    //}

}
