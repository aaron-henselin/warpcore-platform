using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cms;
using Cms.Layout;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.PageComposition;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Web;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets;

namespace WarpCore.Cms
{
    public class ContentPageHandler : IHttpHandler
    {
        public ContentPageHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            var rq = CmsPageRequestContext.Current;
            var activator = new CmsPageContentActivator();
            RenderContentPage(rq,activator);
        }


        private static void RenderContentPage(CmsPageRequestContext rt, CmsPageContentActivator activator)
        {
            var pageBuilder = new PageCompositionBuilder(activator);

            var page = new PageComposition();

            page.RootElement = new UndefinedLayoutPageCompositionElement();
            if (rt.CmsPage.LayoutId != Guid.Empty)
            {

                var layoutRepository = new LayoutRepository();
                var layoutToApply = layoutRepository.GetById(rt.CmsPage.LayoutId);
                
                pageBuilder.ActivateAndPlaceLayoutContent(page, layoutToApply);
            }

            var pageSpecificContent = rt.CmsPage.PageContent;
            if (rt.PageRenderMode == PageRenderMode.PageDesigner)
            {
                var editing = new EditingContextManager();
                var context = editing.GetOrCreateEditingContext(rt.CmsPage);
                pageSpecificContent = context.AllContent;
            }

            var d = page.GetPartialPageRenderingByLayoutBuilderId();

            foreach (var contentItem in pageSpecificContent)
            {
                var placementLayoutBuilderId = contentItem.PlacementLayoutBuilderId ?? SpecialRenderingFragmentContentIds.PageRoot;
                var root = d[placementLayoutBuilderId];
                pageBuilder.ActivateAndPlaceAdHocPageContent(contentItem, root);

            }

            var fragmentMode = rt.PageRenderMode == PageRenderMode.PageDesigner
                ? FragmentRenderMode.PageDesigner
                : FragmentRenderMode.Readonly;

            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page,fragmentMode);


            var compositor = new RenderFragmentCompositor(page,batch);
            var composedPage = compositor.Compose(fragmentMode);

            HttpContext.Current.Response.Write(composedPage.Html);
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