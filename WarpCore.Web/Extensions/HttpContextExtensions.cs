using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
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
            var routeRaw = context.Request["wc-pg"];
            var environmentRaw = context.Request["wc-ce"];
            var contentVersionRaw = context.Request["wc-cv"];
            var viewModeRaw = context.Request["wc-viewmode"];

            PageRenderMode pageRenderMode = PageRenderMode.Readonly;
            if (!string.IsNullOrWhiteSpace(viewModeRaw))
                pageRenderMode = (PageRenderMode)Enum.Parse(typeof(PageRenderMode), viewModeRaw, true);


            ContentEnvironment env = ContentEnvironment.Live;
            if (!string.IsNullOrWhiteSpace(environmentRaw))
            {
                env = (ContentEnvironment)Enum.Parse(typeof(ContentEnvironment),environmentRaw,true);
            }
            else
            {
                if (pageRenderMode == PageRenderMode.PageDesigner)
                    env = ContentEnvironment.Draft;
            }

            decimal contentVersion = 0;
            if (!string.IsNullOrWhiteSpace(contentVersionRaw))
                contentVersion = Convert.ToDecimal(contentVersionRaw);


            var routeContext = new CmsPageRequestContext();
            routeContext.PageRenderMode = pageRenderMode;

            SiteRoute route;

            if (!string.IsNullOrWhiteSpace(routeRaw))
            {
                route = new ContentPageRoute
                {
                    Authority = UriAuthorityFilter.Any,
                    PageId = new Guid(routeRaw)
                };
            }
            else
            {
                var success = CmsRoutes.Current.TryResolveRoute(HttpContext.Current.Request.Url, out route);
            }
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