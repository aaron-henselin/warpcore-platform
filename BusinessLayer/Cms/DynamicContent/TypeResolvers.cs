using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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
                var entityType = WarpCoreIlGenerator.CreateDynamicContentEntity(id);
                Activator.CreateInstance(entityType);

                var repoType = WarpCoreIlGenerator.CreateDynamicRepository(entityType);
                Activator.CreateInstance(repoType);

                _types.Add(id,repoType);
            }

            return _types[id];
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
