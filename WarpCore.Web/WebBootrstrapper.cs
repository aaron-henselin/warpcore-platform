using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;


[assembly: PreApplicationStartMethod(typeof(WebBootstrapper), nameof(WebBootstrapper.PreInitialize))]
namespace WarpCore.Web
{
    public class SettingAttribute : Attribute
    {
    }

    public class IncludeInToolboxAttribute:Attribute
    {
        public string Name { get; set; }
    }

    public static class WebBootstrapper
    {
        public static void BuildUpToolbox()
        {
            var allTypes = typeof(WebBootstrapper).Assembly.GetTypes();
            var toIncludeInToolbox = allTypes.Where(x => x.GetCustomAttributes<IncludeInToolboxAttribute>().Any());

            var mgr = new ToolboxManager();
            var alreadyInToolbox = mgr.Find().ToDictionary(x => x.Name);
            foreach (var typeToInclude in toIncludeInToolbox)
            {
                var includeInToolboxAtr = typeToInclude.GetCustomAttribute<IncludeInToolboxAttribute>();

                ToolboxItem widget;
                if (alreadyInToolbox.ContainsKey(includeInToolboxAtr.Name))
                    widget = alreadyInToolbox[includeInToolboxAtr.Name];
                else
                    widget = new ToolboxItem();

                widget.Name = includeInToolboxAtr.Name;
                widget.FullyQualifiedTypeName = typeToInclude.AssemblyQualifiedName;
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

                    BuildUpToolbox();
                    Thread.Sleep(2000);
                    IsBooted = true;

                });
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