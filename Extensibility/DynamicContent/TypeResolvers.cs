using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Extensibility;
using WarpCore.Cms;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.DynamicContent
{
    public static class EntityTypeResolver
    {
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();


        public static Type ResolveDynamicTypeByInteropId(Guid id)
        {
            if (!_types.ContainsKey(id))
            {
                try
                {
                    var contentTypeRepo = new ContentTypeMetadataRepository();
                    var contentTypeMetadata = contentTypeRepo.Find().Single(x => x.TypeResolverId == id);

                    Type repoType;
                    if (!string.IsNullOrWhiteSpace(contentTypeMetadata.CustomAssemblyQualifiedTypeName))
                        repoType = Type.GetType(contentTypeMetadata.CustomAssemblyQualifiedTypeName);
                    else
                        repoType = Type.GetType(contentTypeMetadata.AssemblyQualifiedTypeName);

                    _types.Add(id, repoType);
                }
                catch (Exception)
                {
                    var entityType = WarpCoreIlGenerator.CreateDynamicContentEntity(id);

                    _types.Add(id, entityType);
                }


            }

            return _types[id];
        }



    }

    public static class RepositoryTypeResolver
    {
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();


        public static Type ResolveDynamicTypeByInteropId(Guid id)
        {
            if (!_types.ContainsKey(id))
            {
                try
                {
                    Type repoType = ResolveMetadataDefinedRepositoryType(id);
                    _types.Add(id, repoType);
                }
                catch (Exception)
                {
                    var entityType = WarpCoreIlGenerator.CreateDynamicContentEntity(id);
                    Activator.CreateInstance(entityType);

                    Type repoType = WarpCoreIlGenerator.CreateDynamicRepository(entityType);
                    Activator.CreateInstance(repoType);

                    _types.Add(id, repoType);
                }
              
              
            }

            return _types[id];
        }

        private static Type ResolveMetadataDefinedRepositoryType(Guid id)
        {
            Type repoType;
            var repositoryMetadataManager = new RepositoryMetadataManager();
            var metadataManager = repositoryMetadataManager.GetRepositoryMetdataByTypeResolverUid(id);
            if (!string.IsNullOrWhiteSpace(metadataManager.CustomAssemblyQualifiedTypeName))
                repoType = Type.GetType(metadataManager.CustomAssemblyQualifiedTypeName);
            else
                repoType = Type.GetType(metadataManager.AssemblyQualifiedTypeName);
            return repoType;
        }
    }


    public class DynamicTypeDefinitionResolver : IDynamicTypeDefinitionResolver
    {
        static Dictionary<Guid, DynamicTypeDefinition> _definitions = new Dictionary<Guid, DynamicTypeDefinition>();

        public DynamicTypeDefinition Resolve(Guid uid)
        {
            if (_definitions.ContainsKey(uid))
                return _definitions[uid];

            var typeExtensions = new ContentInterfaceRepository().Find().Where(x => x.ContentTypeId == uid);

            var dtd = new DynamicTypeDefinition();
            foreach (var extension in typeExtensions)
            {
                dtd.DynamicProperties.AddRange(extension.InterfaceFields);
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

   
}
