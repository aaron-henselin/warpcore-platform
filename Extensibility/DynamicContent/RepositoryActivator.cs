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
                var metadataManager = new RepositoryMetadataManager();
                var repositoryMetadata = metadataManager.GetRepositoryMetdataByTypeResolverUid(id);
                if (repositoryMetadata.IsDynamic)
                {
                    var entityType = DynamicTypeBuilder.TypeBuilderHelper.CreateDynamicContentEntity(id);
                    var repoType = DynamicTypeBuilder.TypeBuilderHelper.CreateDynamicRepository(entityType);
                    _types.Add(id, repoType);
                }
                else
                {
                    Type repoType = null;
                    var metadata = metadataManager.GetRepositoryMetdataByTypeResolverUid(id);
                    if (metadata.CustomAssemblyQualifiedTypeName != null)
                        repoType= Type.GetType(metadata.CustomAssemblyQualifiedTypeName);

                    if (metadata.AssemblyQualifiedTypeName != null)
                        repoType= Type.GetType(metadata.AssemblyQualifiedTypeName);

                    _types.Add(id, repoType);
                }

                
            }

            return _types[id];
        }
    }
    
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
                var assemblyName = new AssemblyName(DynamicTypeAssembly);
                var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
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
    public class ContentInterface : UnversionedContentEntity
    {
        public Guid TypeResolverUid { get; set; }

        public string Name { get; set; }

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

            var typeExtensions = new ContentInterfaceRepository().Find().Where(x => x.TypeResolverUid == uid);

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

    public class FriendlyTypeInfo : UnversionedContentEntity
    {
        public string FriendlyName { get; set; }

    }

    public class FiendlyTypeInfoRepository : UnversionedContentRepository<FriendlyTypeInfo>
    {

    }

    public class ContentInterfaceRepository : UnversionedContentRepository<ContentInterface>
    {
        public ContentInterface GetCustomFieldsContentInterface(Guid uid)
        {
            return Find().Single(x => x.TypeResolverUid == uid 
                                   && x.Name == KnownTypeExtensionNames.CustomFields);
        }
    }

}
