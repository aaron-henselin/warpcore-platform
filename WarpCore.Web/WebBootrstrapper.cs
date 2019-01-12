﻿using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Modules.Cms.Features.Context;
using Platform_Hosting_AspNet;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;


[assembly: PreApplicationStartMethod(typeof(WebBootstrapper), nameof(WebBootstrapper.PreInitialize))]

namespace WarpCore.Web
{


    //public static class BootEvents
    //{
    //    private static ConcurrentBag<Action> _afterBootActions = new ConcurrentBag<Action>();
    //    public static void RegisterSiteBootAction(Action a)
    //    {
    //        _afterBootActions.Add(a);
    //    }

    //    internal static void RunSiteBootActions()
    //    {
    //        foreach (var action in _afterBootActions)
    //            action();
    //    }

    //}

    public class SiteBootCompleted:IDomainEvent
    {
    }



    public static class WebBootstrapper
    {
        public static void BuildUpDomainEvents()
        {
            DomainEvents.Subscribe<SiteStructureChanged>(x => CmsRoutes.RegenerateAllRoutes());
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
                    BuildUpDomainEvents();

                    Dependency.Register<IDynamicTypeDefinitionResolver>(typeof(DynamicTypeDefinitionResolver));
                    
                    ExtensibleTypeConverter.TypeConverters.Add(new WarpCorePageUriTypeConverter());

                    ExtensibilityBootstrapper.PreloadPluginAssembliesFromFileSystem(AppDomain.CurrentDomain);
                    ExtensibilityBootstrapper.RegisterExtensibleTypesWithApi(AppDomain.CurrentDomain);
                   
                    ToolboxBootstrapper.RegisterToolboxItemsWithApi(AppDomain.CurrentDomain);
                    
                    IsBooted = true;

                }).ContinueWith(x =>
                
                    DomainEvents.Raise(new SiteBootCompleted())
                );
            }
        }


       

        public static void PreInitialize()
        {
            DynamicModuleUtility.RegisterModule(typeof(CmsPageBuilderHttpModule));

            //aspnet pipeline
            Dependency.Register<IHttpRequest>(() => new AspNetHttpRequest());
            Dependency.Register<IWebServer>(() => new AspNetWebServer());
            Dependency.Register<IHttpItems>(() => new AspNetItems());

            Dependency.Register<CmsPageRequestContext>(() => CmsPageBuilderHttpModule.RequestContext);
            Dependency.Register<UriBuilderContext>(() => HttpContext.Current.ToUriBuilderContext());



            CmsRouteRegistrar.RegisterDynamicRoutes();
        }



    }
}