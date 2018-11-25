﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility
{
    internal static class TypeSearcher
    {
        public static IReadOnlyCollection<ExtensibleRepositoryDescription> FindExtensibleRepositoryTypes(IReadOnlyCollection<Type> allTypes)
        {
            return allTypes
                .HavingAttribute<ExposeToWarpCoreApi>()
                .Where(x => typeof(IContentRepository).IsAssignableFrom(x))
                .DistinctBy(x => x.AssemblyQualifiedName)
                .Select(x => new ExtensibleRepositoryDescription
                {
                    RepositoryType = x,
                    Uid = x.GetCustomAttribute<ExposeToWarpCoreApi>().TypeUid
                })
                .ToList();           
        }

        public static IReadOnlyCollection<Type> FilterToExtensibleEntityTypes(IReadOnlyCollection<Type> allTypes)
        {
            return allTypes.HavingAttribute<WarpCoreEntityAttribute>()
                .Where(x => typeof(WarpCoreEntity).IsAssignableFrom(x))
                .DistinctBy(x => x.AssemblyQualifiedName)
                .ToList();
        }
    }

    internal class ExtensibleRepositoryDescription
    {
        public Guid Uid { get; set; }
        public Type RepositoryType { get; set; }
    }
    public class IsWarpCorePluginAssemblyAttribute : Attribute
    {
    }


    public class ExtensibilityBootstrapper
    {
        public static void PreloadPluginAssembliesFromFileSystem(AppDomain appDomain)
        {
            AssemblyLoader.LoadAssemblies(appDomain, asm => asm.GetCustomAttribute<IsWarpCorePluginAssemblyAttribute>() != null);
        }

        public static void RegisterExtensibleTypesWithApi(AppDomain domain)
        {
            var assemblies = domain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();

            var repositoryDescriptions = TypeSearcher.FindExtensibleRepositoryTypes(allTypes);



            foreach (var repositoryDescription in repositoryDescriptions)
                BuildUpRepositoryMetadata(repositoryDescription);

            //Break this one out later.
            var entities = TypeSearcher.FilterToExtensibleEntityTypes(allTypes);
            var typeExtensionRepo = new ContentInterfaceRepository();
            foreach (var entityType in entities)
            {
                var repositoryUid = entityType.GetCustomAttribute<WarpCoreEntityAttribute>();
                var preexisting = typeExtensionRepo.Find().SingleOrDefault(x => x.ContentTypeId == repositoryUid.TypeExtensionUid && x.InterfaceName == KnownTypeExtensionNames.CustomFields);
                if (preexisting == null)
                    typeExtensionRepo.Save(new ContentInterface
                    {
                        ContentTypeId = repositoryUid.TypeExtensionUid,
                        InterfaceName = KnownTypeExtensionNames.CustomFields
                    });

            }
        }

        private static void BuildUpRepositoryMetadata(ExtensibleRepositoryDescription repositoryDescription)
        {

            var respositoryManager = new RepositoryMetadataManager();
            var preexistingMetadata = respositoryManager.Find($"{nameof(RepositoryMetdata.ApiId)} eq '{repositoryDescription.Uid}'").SingleOrDefault();

            RepositoryMetdata metadata = new RepositoryMetdata();
            if (preexistingMetadata != null)
                metadata = preexistingMetadata;

            metadata.ApiId = repositoryDescription.Uid;
            metadata.AssemblyQualifiedTypeName = repositoryDescription.RepositoryType.AssemblyQualifiedName;
            respositoryManager.Save(metadata);
            


        }
    }



}
