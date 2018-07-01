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

        

        public static CmsPageBuilderContext ToCmsPageBuilderContext(this HttpContext context)
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

            var success = CmsRoutes.Current.TryResolveRoute(HttpContext.Current.Request.Url, out var route);
            if (!success || route.PageId == null)
                return new CmsPageBuilderContext();

            var cmsPageVersions = new PageRepository().FindContentVersions(By.ContentId(route.PageId.Value), env).Result;

            CmsPage cmsPage;
            if (env == ContentEnvironment.Archive)
                cmsPage = cmsPageVersions.Single(x => x.ContentVersion == contentVersion);
            else
                cmsPage = cmsPageVersions.Single();

            if (PageType.ContentPage != cmsPage.PageType)
                return new CmsPageBuilderContext();

            return new CmsPageBuilderContext
            {
                Page = cmsPage,
                ViewMode = viewMode
            };
        }
    }
}