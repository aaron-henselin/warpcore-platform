﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cms.Layout;
using Cms_PageDesigner_Context;
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
            var activator = new CmsPageContentActivator();
            RenderContentPage(rq,activator);
        }



        public static PageLayout GetLayoutStructure(Layout layout)
        {
            var layoutRepo = new LayoutRepository();

            PageLayout ln = null;
            if (layout.ParentLayoutId != null)
            {
                var parentLayout = layoutRepo.GetById(layout.ParentLayoutId.Value);
                ln = GetLayoutStructure(parentLayout);
            }

            return new PageLayout
            {
                AllContent = layout.PageContent.Select(x => x.ToPresentationElement()).ToList(),
                MasterPagePath = layout.MasterPagePath,
                Name = layout.Name,
                ParentLayout = ln
            };
        }



        private static void RenderContentPage(CmsPageRequestContext rt, CmsPageContentActivator activator)
        {
            var pageBuilder = new PageComposer(activator);

            var page = new PageComposition();

            page.RootElement = new UndefinedLayoutPageCompositionElement();
            if (rt.CmsPage.LayoutId != Guid.Empty)
            {

                var layoutRepository = new LayoutRepository();
                var layoutToApply = layoutRepository.GetById(rt.CmsPage.LayoutId);
                
                pageBuilder.AddLayoutContent(page, GetLayoutStructure(layoutToApply) );
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

                var presentationElement = contentItem.ToPresentationElement();
                pageBuilder.AddAdHocContent(presentationElement, root);
            }



            var fragmentMode = rt.PageRenderMode == PageRenderMode.PageDesigner
                ? FragmentRenderMode.PageDesigner
                : FragmentRenderMode.Readonly;

            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page,fragmentMode);

            var cache = Dependency.Resolve<CmsPageContentOutputCacheProvider>();
            foreach (var item in page.RootElement.GetAllDescendents())
            {
                if (!string.IsNullOrWhiteSpace(item.CacheKey))
                    cache.AddToCache(item.CacheKey, new CachedPageContentOutput
                    {
                        InternalLayout = (item as IHasInternalLayout)?.GetInternalLayout(),
                        RenderingResult = batch.RenderingResults[item.ContentId]
                    });
            }


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