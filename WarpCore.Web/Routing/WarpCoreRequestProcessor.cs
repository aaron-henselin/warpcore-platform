using System.Collections.Generic;
using System.Linq;
using System.Web;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms.Routing;
using WarpCore.Platform.Kernel;
using WarpCore.Web.Extensions;

namespace WarpCore.Cms
{
    public static class PresentationElementHelpers
    {
        public static PageContent ToPresentationElement(this CmsPageContent content)
        {
            return new PageContent
            {
                Id = content.Id,
                AllContent = content.AllContent.Select(ToPresentationElement).ToList(),
                Order = content.Order,
                Parameters = content.Parameters,
                PlacementContentPlaceHolderId = content.PlacementContentPlaceHolderId,
                PlacementLayoutBuilderId = content.PlacementLayoutBuilderId,
                WidgetTypeCode = content.WidgetTypeCode,
            };
        }
    }


    public class ContentPageHandler : IHttpHandler
    {
        public ContentPageHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            var rq = CmsPageRequestContext.Current;
          
            RenderContentPage(rq);
        }



        


        private static void RenderContentPage(CmsPageRequestContext rt)
        {
            var builder = new PageCompositionBuilder();
            var pageComposition = builder.CreatePageComposition(rt.CmsPage, rt.PageRenderMode);

            var fragmentMode = rt.PageRenderMode == PageRenderMode.PageDesigner
                ? FragmentRenderMode.PageDesigner
                : FragmentRenderMode.Readonly;

            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(pageComposition, fragmentMode);

            var cache = Dependency.Resolve<CmsPageContentOutputCacheProvider>();
            foreach (var item in pageComposition.RootElement.GetAllDescendents())
            {
                if (!string.IsNullOrWhiteSpace(item.CacheKey))
                    cache.AddToCache(item.CacheKey, new CachedPageContentOutput
                    {
                        InternalLayout = (item as IHasInternalLayout)?.GetInternalLayout(),
                        RenderingResult = batch.RenderingResults[item.ContentId]
                    });
            }


            var compositor = new RenderFragmentCompositor(pageComposition,batch);

            var writer = new HtmlOnlyComposedHtmlWriter();
            compositor.WriteComposedFragments(fragmentMode,writer);
          
            HttpContext.Current.Response.Write(writer.ToString());
        }

        public bool IsReusable { get; } = false;
    }



    public  class WarpCoreRequestProcessor
    {
        private static void ProcessRequestForContentPage(HttpContext context, CmsPage cmsPage)
        {
            string transferUrl;

            if (!string.IsNullOrWhiteSpace(cmsPage.PhysicalFile))
            {
                transferUrl = cmsPage.PhysicalFile;
                context.RewritePath(transferUrl, false);
            }
            else
            {
           
                
            }
            

        }


        private string CreateUrl(SiteRoute transferRoute, HttpContext httpContext, IDictionary<string,string> parameters)
        {
            var uriBuilderContext = httpContext.ToUriBuilderContext();
            var uriBuilder = new CmsUriBuilder(uriBuilderContext);
            return uriBuilder.CreateUriForRoute(transferRoute, UriSettings.Default, parameters).ToString();
   
        }

        public void ProcessRequest(HttpContext context, CmsPageRequestContext pageRequestContext)
        {
            //var siteRoute = (SiteRoute)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
            //var contentEnvironment = (ContentEnvironment)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.ContentEnvironmentToken];

            var siteRoute = pageRequestContext.Route;

            if (siteRoute is RedirectPageRoute)
            {
                var redirectRoute = (RedirectPageRoute)siteRoute;
                ProcessRedirectPageRequest(context, redirectRoute);
                return;
            }


            if (siteRoute is GroupingPageRoute)
            {
                var redirectRoute = (GroupingPageRoute)siteRoute;
                ProcessGroupingPageRequest(context, redirectRoute);
                return;
            }

          
            ProcessRequestForContentPage(context, pageRequestContext.CmsPage);

        }

        private void ProcessGroupingPageRequest(HttpContext context, GroupingPageRoute redirectRoute)
        {
            if (redirectRoute.InternalRedirectPageId == null)
                throw new HttpException(404, "No page id.");

            SiteRoute redirectToRoute;
            CmsRoutes.Current.TryResolveRoute(redirectRoute.InternalRedirectPageId.Value, out redirectToRoute);

            var redirectUrl = CreateUrl(redirectToRoute, context, null);
            context.Response.Redirect(redirectUrl);
        }

        private void ProcessRedirectPageRequest(HttpContext context, RedirectPageRoute redirectRoute)
        {
            if (!string.IsNullOrWhiteSpace(redirectRoute.RedirectExternalUrl))
            {
                context.Response.Redirect(redirectRoute.RedirectExternalUrl);
                return;
            }

            if (redirectRoute.InternalRedirectPageId == null)
                throw new HttpException(404,"No page id.");

            SiteRoute redirectToRoute;
            CmsRoutes.Current.TryResolveRoute(redirectRoute.InternalRedirectPageId.Value, out redirectToRoute);

            var redirectUrl = CreateUrl(redirectToRoute, context,redirectRoute.InternalRedirectParameters);
            context.Response.Redirect(redirectUrl);
            return;
        }
    }
}