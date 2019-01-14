using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Configuration
{

    public class CmsModuleInitializer : IModuleInitializer
    {
        public void InitializeModule(CmsConfiguration cmsConfiguration)
        {
            //todo: get this moved.
            var fragmentRenderers = cmsConfiguration.SupportedRenderingEngines.Select(x => x.FragmentRenderer).ToList();
            var pageCompositionFactories = cmsConfiguration.SupportedRenderingEngines.Select(x => x.PageCompositionFactory).ToList();

            Dependency.RegisterMultiple<IPageCompositionElementFactory>(pageCompositionFactories);
            Dependency.RegisterMultiple<IFragmentRenderer>(fragmentRenderers);

            Dependency.ResolveMultiple<IFragmentRenderer>();

            GlobalConfiguration.Configuration.Services.Replace(typeof(IAssembliesResolver),new CustomAssemblyResolver());
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.EnsureInitialized();
        }

        public void InitializeModule()
        {
            InitializeModule(CmsConfiguration.Current);
        }

        public static class WebApiConfig
        {
            public static void Register(HttpConfiguration config)
            {
                config.MapHttpAttributeRoutes();



                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional });
                
            }
        }

        public class CustomAssemblyResolver : IAssembliesResolver
        {
            public ICollection<Assembly> GetAssemblies()
            {
                return AppDomain.CurrentDomain.GetAssemblies();
            }
        }
    }
}


