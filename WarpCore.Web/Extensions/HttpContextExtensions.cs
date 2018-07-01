using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Web.Extensions
{

    public static class HttpContextExtensions
    {
        public static UriBuilderContext ToUriBuilderContext(this HttpContext context)
        {
            var url = context.Request.Url;
            return new UriBuilderContext()
            {
                Authority = url.Authority,
                AbsolutePath = new Uri(url.AbsolutePath,UriKind.Relative),
                IsSsl = url.Scheme == "https"
            };
        }

        

        public static CmsPageRequestContext ToCmsRouteContext(this HttpContext context)
        {
            var environmentRaw = context.Request["wc-ce"];
            var contentVersionRaw = context.Request["wc-cv"];
            var viewModeRaw = context.Request["wc-viewmode"];

            ContentEnvironment env = ContentEnvironment.Live;
            if (!string.IsNullOrWhiteSpace(environmentRaw))
            {
                env = (ContentEnvironment)Enum.Parse(typeof(ContentEnvironment),environmentRaw,true);
            }

            decimal contentVersion = 0;
            if (!string.IsNullOrWhiteSpace(contentVersionRaw))
                contentVersion = Convert.ToDecimal(contentVersionRaw);

            ViewMode viewMode = ViewMode.Default;
            if (!string.IsNullOrWhiteSpace(viewModeRaw))
                viewMode = (ViewMode) Enum.Parse(typeof(ViewMode), viewModeRaw, true);

            var routeContext = new CmsPageRequestContext();
            routeContext.ViewMode = viewMode;

            var success = CmsRoutes.Current.TryResolveRoute(HttpContext.Current.Request.Url, out var route);
            routeContext.Route = route;

            if (route?.PageId == null)
                return routeContext;

            var cmsPageVersions = new PageRepository().FindContentVersions(By.ContentId(route.PageId.Value), env).Result;

            if (env == ContentEnvironment.Archive)
                routeContext.CmsPage = cmsPageVersions.Single(x => x.ContentVersion == contentVersion);
            else
                routeContext.CmsPage = cmsPageVersions.Single();

            return routeContext;
            //if (PageType.ContentPage != cmsPage.PageType)
            //    return new CmsRouteContext();

            //return new CmsRouteContext
            //{
            //    CmsPage = cmsPage,
            //    ViewMode = viewMode
            //};
        }
    }
}