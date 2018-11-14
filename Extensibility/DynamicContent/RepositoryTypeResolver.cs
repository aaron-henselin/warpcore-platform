using System;
using System.Collections.Generic;

namespace WarpCore.Platform.Extensibility.DynamicContent
{
    public static class RepositoryTypeResolver
    {
        private static Dictionary<Guid, Type> _types = new Dictionary<Guid, Type>();


        public static Type ResolveTypeByApiId(Guid id)
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
}