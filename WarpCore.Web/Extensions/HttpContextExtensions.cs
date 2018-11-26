using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Platform.Orm;

namespace WarpCore.Web.Extensions
{
    public static class PageDesignerUriComponents
    {
        public const string PageId = "wc-pg";
        public const string SiteId = "wc-st";
        public const string ContentEnvironment = "wc-ce";
        public const string ViewMode = "wc-viewmode";
        public const string ContentVersion = "wc-cv";
    }


    public static class HttpContextExtensions
    {
        public static DynamicFormRequestContext ToDynamicFormRequestContext(this HttpContext context)
        {
            Guid? guid = null;
            var contentIdRaw = context.Request.QueryString[nameof(DynamicFormRequestContext.ContentId)];
            if (!string.IsNullOrEmpty(contentIdRaw))
                guid = new Guid(contentIdRaw);

            return new DynamicFormRequestContext
            {
                ContentId = guid,
                DefaultValues = DefaultValueCollection.FromString(context.Request.QueryString[nameof(DynamicFormRequestContext.DefaultValues)])
            };
        }


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

        

        internal static CmsPageRequestContext ToCmsRouteContext(this HttpContext context)
        {
            var routeRaw = context.Request[PageDesignerUriComponents.PageId];
            var environmentRaw = context.Request[PageDesignerUriComponents.ContentEnvironment];
            var contentVersionRaw = context.Request[PageDesignerUriComponents.ContentVersion];
            var viewModeRaw = context.Request[PageDesignerUriComponents.ViewMode];
            var siteRaw = context.Request[PageDesignerUriComponents.SiteId];

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
                    PageId = new Guid(routeRaw),
                    SiteId = new Guid(siteRaw),
                };
            }
            else
            {
                var success = CmsRoutes.Current.TryResolveRoute(HttpContext.Current.Request.Url, out route);
            }
            routeContext.Route = route;
            

            if (route?.PageId == null)
                return routeContext;

            var cmsPageVersions = new CmsPageRepository().FindContentVersions(By.ContentId(route.PageId.Value), env).Result;

            if (env == ContentEnvironment.Archive)
                routeContext.CmsPage = cmsPageVersions.Single(x => x.ContentVersion == contentVersion);
            else
                routeContext.CmsPage = cmsPageVersions.Single();

            return routeContext;

        }
    }
}