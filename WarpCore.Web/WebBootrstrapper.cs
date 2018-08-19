using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Cms.DynamicContent;
using Cms.Toolbox;
using Framework;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using TinyIoC;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;


[assembly: PreApplicationStartMethod(typeof(WebBootstrapper), nameof(WebBootstrapper.PreInitialize))]

namespace WarpCore.Web
{
    public static class ConditionalAssemblyLoader
    {


        public static void LoadAssemblies(Func<Assembly,bool> condition)
        {
            List<string> filesToLoad = new List<string>();

            var setup = AppDomain.CurrentDomain.SetupInformation;
            var binariesToCheck =Directory.GetFiles(setup.PrivateBinPath, "*.dll").ToList();

            var alreadyLoaded = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(x => !x.IsDynamic)
                .Select(x => x.Location);

            binariesToCheck = binariesToCheck.Except(alreadyLoaded).ToList();


            ////var assembliesDir = setup.PrivateBinPathProbe != null 
            ////    ? setup.PrivateBinPath : setup.ApplicationBase;

            //AppDomain oTempAppDomain = AppDomain.CreateDomain("tempAppDomain");

            //foreach (var binaryToCheck in binariesToCheck)
            //try
            //{
            //    AssemblyLoader al =
            //        (AssemblyLoader) oTempAppDomain
            //            .CreateInstanceAndUnwrap(typeof(AssemblyLoader).Assembly.FullName,
            //            typeof(AssemblyLoader).FullName);

            //    var isPlugin = al.IsPluginAssembly(binaryToCheck,condition);
            //    if (isPlugin)
            //        filesToLoad.Add(binaryToCheck);
            //}
            //catch (Exception ex)
            //{
                
            //}

            //AppDomain.Unload(oTempAppDomain);

            foreach (var file in binariesToCheck)
            {
                Assembly.LoadFile(file);
            }
        }


        public class AssemblyLoader : MarshalByRefObject
        {
            public bool IsPluginAssembly(string assembly, Func<Assembly, bool> condition)
            {
                Assembly a = Assembly.ReflectionOnlyLoadFrom(assembly);
                return condition(a);
            }
        }


    }

    public static class BootEvents
    {
        private static ConcurrentBag<Action> _afterBootActions = new ConcurrentBag<Action>();
        public static void RegisterSiteBootAction(Action a)
        {
            _afterBootActions.Add(a);
        }

        internal static void RunSiteBootActions()
        {
            foreach (var action in _afterBootActions)
                action();
        }

    }



    public static class WebBootstrapper
    {

        public static void PreloadPlugins()
        {
            ConditionalAssemblyLoader.LoadAssemblies(asm => asm.GetCustomAttribute<IsWarpCorePluginAssemblyAttribute>() != null);
        }

        public static void BuildUpRepositoryMetadata()
        {
            var respositoryManager = new RepositoryMetadataManager();
            var preexistingMetadata = respositoryManager.Find().ToDictionary(x => x.FormInteropUid);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();
            
            var repositories = allTypes.HavingAttribute<FormDesignerInteropAttribute>().Where(x => typeof(IContentRepository).IsAssignableFrom(x));
            var entities = allTypes.HavingAttribute<SupportsCustomFieldsAttribute>().Where(x => typeof(CosmosEntity).IsAssignableFrom(x));

            foreach (var repoType in repositories)
            {
                var repositoryUid = repoType.GetCustomAttribute<FormDesignerInteropAttribute>();
                var uid = repositoryUid.TypeUid;
                var alreadyExists = preexistingMetadata.ContainsKey(uid);

                RepositoryMetdata metadata = new RepositoryMetdata();
                if (alreadyExists)
                    metadata = preexistingMetadata[uid];

                metadata.FormInteropUid = uid;
                metadata.AssemblyQualifiedTypeName = repoType.AssemblyQualifiedName;
                respositoryManager.Save(metadata);
            }

            var typeExtensionRepo = new ContentInterfaceRepository();
            foreach (var entityType in entities)
            {
                var repositoryUid = entityType.GetCustomAttribute<SupportsCustomFieldsAttribute>();
                var preexisting = typeExtensionRepo.Find().SingleOrDefault(x => x.TypeResolverUid == repositoryUid.TypeExtensionUid && x.Name == KnownTypeExtensionNames.CustomFields);
                if (preexisting == null)
                    typeExtensionRepo.Save(new ContentInterface
                    {
                        TypeResolverUid = repositoryUid.TypeExtensionUid,
                        Name = KnownTypeExtensionNames.CustomFields
                    });
                
            }

        }

        public static void BuildUpToolbox()
        {
            var allTypes = typeof(WebBootstrapper).Assembly.GetTypes();
            var toIncludeInToolbox = allTypes
                                        .Select(ToolboxMetadataReader.ReadMetadata)
                                        .Where(x => x != null);

            var mgr = new ToolboxManager();
            var alreadyInToolbox = mgr.Find().ToDictionary(x => x.WidgetUid);
            foreach (var discoveredToolboxItem in toIncludeInToolbox)
            {
                //var includeInToolboxAtr = typeToInclude.GetCustomAttribute<IncludeInToolboxAttribute>();

                ToolboxItem widget;
                if (alreadyInToolbox.ContainsKey(discoveredToolboxItem.WidgetUid))
                    widget = alreadyInToolbox[discoveredToolboxItem.WidgetUid];
                else
                    widget = new ToolboxItem();

                widget.WidgetUid = discoveredToolboxItem.WidgetUid;
                widget.FriendlyName = discoveredToolboxItem.FriendlyName;
                widget.AssemblyQualifiedTypeName = discoveredToolboxItem.AssemblyQualifiedTypeName;
                widget.Category = discoveredToolboxItem.Category;
                mgr.Save(widget);
            }

        }

        public static bool IsBooted { get; private set; }

        private static readonly object _bootStartedSync = new object();
        private static bool _bootingStarted;

      
        public static void EnsureSiteBootHasBeenStarted()
        {
            lock (_bootStartedSync)
            {
                if (_bootingStarted)
                    return;

                _bootingStarted = true;
                Task.Run(() =>
                {
                    Dependency.Register<IDynamicTypeDefinitionResolver>(typeof(DynamicTypeDefinitionResolver));
                    
                    PreloadPlugins();
                    BuildUpRepositoryMetadata();
                    BuildUpToolbox();
                    
                    IsBooted = true;

                }).ContinueWith(x =>BootEvents.RunSiteBootActions());
            }
        }




        public static void PreInitialize()
        {
            DynamicModuleUtility.RegisterModule(typeof(CmsPageBuilderHttpModule));

            Dependency.Register<CmsPageRequestContext>(() => HttpContext.Current.ToCmsRouteContext());
            Dependency.Register<UriBuilderContext>(() => HttpContext.Current.ToUriBuilderContext());

            CmsRouteRegistrar.RegisterDynamicRoutes();
        }
    }
}