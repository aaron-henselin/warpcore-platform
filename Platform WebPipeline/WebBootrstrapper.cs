using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

using Platform_WebPipeline;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;



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

        internal static void EnsureSiteBootHasBeenStarted()
        {
            lock (_bootStartedSync)
            {
                if (_bootingStarted)
                    return;

                _bootingStarted = true;
                Task.Run(() =>
                {
                    Boot();
                    IsBooted = true;
                }).ContinueWith(x =>
                
                    DomainEvents.Raise(new SiteBootCompleted())
                );
            }
        }

        private static void Boot()
        {
            BuildUpDomainEvents();

            Dependency.Register<IDynamicTypeDefinitionResolver>(typeof(DynamicTypeDefinitionResolver));

            ExtensibleTypeConverter.TypeConverters.Add(new WarpCorePageUriTypeConverter());

            ExtensibilityBootstrapper.PreloadPluginAssembliesFromFileSystem(AppDomain.CurrentDomain);
            ExtensibilityBootstrapper.RegisterExtensibleTypesWithApi(AppDomain.CurrentDomain);

            ToolboxBootstrapper.RegisterToolboxItemsWithApi(AppDomain.CurrentDomain);

            ExtensibilityBootstrapper.InitializeModules(AppDomain.CurrentDomain);
        }


        public static void PreInitializeWebStack(WebStackConfiguration webStackConfiguration)
        {

            Dependency.Register<IHttpRequest>(webStackConfiguration.HttpRequestType);
            Dependency.Register<IWebServer>(webStackConfiguration.WebServerType);
            Dependency.Register<IPerRequestItems>(webStackConfiguration.PerRequestItemsType);
            Dependency.Register<IRouteData>(webStackConfiguration.RouteDataType);

            webStackConfiguration.RegisterHttpModules();
           
            Dependency.Register<CmsPageRequestContext>(() => WebPipeline.CurrentRequest);
            Dependency.Register<UriBuilderContext>(() => WebDependencies.Request.ToUriBuilderContext());
        }



    }

    public abstract class WebStackConfiguration
    {
        public Type HttpRequestType { get; set; }
        public Type WebServerType { get; set; }
        public Type PerRequestItemsType { get; set; }
        public Type RouteDataType { get; set; }

        public abstract void RegisterHttpModules();

        public void HttpRequest<THttpRequest>() where THttpRequest : IHttpRequest
        {
            HttpRequestType = typeof(THttpRequest);
        }

        public void WebServer<TWebServer>() where TWebServer : IWebServer
        {
            WebServerType = typeof(TWebServer);
        }

        public void PerRequestItems<TPerRequestItems>() where TPerRequestItems : IPerRequestItems
        {
            PerRequestItemsType = typeof(TPerRequestItems);
        }


        public void RouteData<TRouteData>() where TRouteData : IRouteData
        {
            RouteDataType = typeof(TRouteData);
        }

    }


    //public class CmsRouteRegistrar
    //{
    //    public static void RegisterDynamicRoutes()
    //    {
    //        RouteTable.Routes.RouteExistingFiles = false;
    //        RouteTable.Routes.Ignore("{file}.js");

    //        //RouteTable.Routes.Add("BlazorModules", new Route("{*pathInfo}", new RouteValueDictionary(), new RouteValueDictionary(), new BlazorHostRouteHandler()));
    //        //var constraints = new RouteValueDictionary();
    //        //constraints.Add("url", new AppUrlConstraint());
    //        //RouteTable.Routes.Add("BlazorModules", new Route("{url}.js",defaults: new RouteValueDictionary(), constraints:constraints,routeHandler: new BlazorHostRouteHandler()));


    //    }
    //}
}