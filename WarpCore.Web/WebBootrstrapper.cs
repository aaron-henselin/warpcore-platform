using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;

[assembly: PreApplicationStartMethod(typeof(WebBootstrapper), nameof(WebBootstrapper.Bootstrap))]
namespace WarpCore.Web
{
    public static class WebBootstrapper
    {
        public static void Bootstrap()
        {
            DynamicModuleUtility.RegisterModule(typeof(CmsPageBuilderHttpModule));

            Dependency.Register(() => HttpContext.Current.ToCmsPageBuilderContext());
            Dependency.Register(() => HttpContext.Current.ToUriBuilderContext());

            CmsRouteRegistrar.RegisterDynamicRoutes();
        }
    }
}