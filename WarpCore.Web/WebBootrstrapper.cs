using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;


[assembly: PreApplicationStartMethod(typeof(WebBootstrapper), nameof(WebBootstrapper.Bootstrap))]
namespace WarpCore.Web
{
    public class IncludeInToolboxAttribute:Attribute
    {
        public string Name { get; set; }
    }

    public static class WebBootstrapper
    {
        private static void BuildUpToolbox()
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
                widget.FullyQualifiedTypeName = typeToInclude.FullName;
                mgr.Save(widget);
            }

        }

        public static void Bootstrap()
        {
            BuildUpToolbox();

            DynamicModuleUtility.RegisterModule(typeof(CmsPageBuilderHttpModule));

            Dependency.Register<CmsPageRequestContext>(() => HttpContext.Current.ToCmsRouteContext());
            Dependency.Register<UriBuilderContext>(() => HttpContext.Current.ToUriBuilderContext());

            CmsRouteRegistrar.RegisterDynamicRoutes();
        }
    }
}