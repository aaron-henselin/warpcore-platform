using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility.DynamicContent
{
    public class DynamicTypeDefinitionResolver : IDynamicTypeDefinitionResolver
    {
        static Dictionary<Guid, DynamicTypeDefinition> _definitions = new Dictionary<Guid, DynamicTypeDefinition>();

        public DynamicTypeDefinition Resolve(Guid uid)
        {
            if (_definitions.ContainsKey(uid))
                return _definitions[uid];

            var typeExtensions = new ContentInterfaceRepository().Find().Where(x => x.ContentTypeId == uid);
            var metadata = new ContentTypeMetadataRepository().Find().Where(x => x.TypeResolverId == uid).SingleOrDefault();
            var dtd = new DynamicTypeDefinition();
            dtd.TitleProperty = metadata?.TitleProperty;
            foreach (var extension in typeExtensions)
            {
                dtd.DynamicProperties.AddRange(extension.InterfaceFields);
            }

            _definitions.Add(uid, dtd);
            return _definitions[uid];
        }

        public DynamicTypeDefinition Resolve(Type type)
        {
            var cosmosEntityAttribute = type.GetCustomAttribute<WarpCoreEntityAttribute>();
            if (cosmosEntityAttribute != null)
                return Resolve(cosmosEntityAttribute.TypeExtensionUid);

            return null;
        }
    }
}