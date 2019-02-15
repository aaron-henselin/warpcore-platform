using System;
using System.Linq;
using System.Web;
using Cms_PageDesigner_Context;
using Platform_WebPipeline;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace WarpCore.Web.Extensions
{





    //public static class HttpContextExtensions
    //{
    //    public static DynamicFormRequestContext ToDynamicFormRequestContext(this HttpContext context)
    //    {
    //        Guid? guid = null;
    //        var contentIdRaw = context.Request.QueryString[nameof(DynamicFormRequestContext.ContentId)];
    //        if (!string.IsNullOrEmpty(contentIdRaw))
    //            guid = new Guid(contentIdRaw);

    //        return new DynamicFormRequestContext
    //        {
    //            ContentId = guid,
    //            DefaultValues = DefaultValueCollection.FromString(context.Request.QueryString[nameof(DynamicFormRequestContext.DefaultValues)])
    //        };
    //    }
    //}


    public class CmsPageRequestContextBuilder
    {
        public CmsPageRequestContext Build(IHttpRequest httpRequest)
        {
            var routeRaw = httpRequest.QueryString[PageDesignerUriComponents.PageId];
            var environmentRaw = httpRequest.QueryString[PageDesignerUriComponents.ContentEnvironment];
            var contentVersionRaw = httpRequest.QueryString[PageDesignerUriComponents.ContentVersion];
            var viewModeRaw = httpRequest.QueryString[PageDesignerUriComponents.ViewMode];
            var siteRaw = httpRequest.QueryString[PageDesignerUriComponents.SiteId];

            PageRenderMode pageRenderMode = PageRenderMode.Readonly;
            if (!string.IsNullOrWhiteSpace(viewModeRaw))
                pageRenderMode = (PageRenderMode)Enum.Parse(typeof(PageRenderMode), viewModeRaw, true);


            ContentEnvironment env = ContentEnvironment.Live;
            if (!string.IsNullOrWhiteSpace(environmentRaw))
            {
                env = (ContentEnvironment)Enum.Parse(typeof(ContentEnvironment), environmentRaw, true);
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
                var success = CmsRoutes.Current.TryResolveRoute(httpRequest.Uri, out route);
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