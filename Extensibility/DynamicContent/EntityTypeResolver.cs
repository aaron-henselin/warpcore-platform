using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.Platform.Extensibility.DynamicContent
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
}
