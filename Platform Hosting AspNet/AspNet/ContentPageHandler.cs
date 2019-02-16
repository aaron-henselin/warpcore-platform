using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using Modules.Cms.Features.Presentation.PageComposition;
using Platform_WebPipeline;
using Platform_WebPipeline.Requests;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Platform_Hosting_AspNet.AspNet
{
    public class ContentPageHandler : IHttpHandler
    {
        public ContentPageHandler()
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            var rq = WebPipeline.CurrentRequest;
            RenderContentPage(rq);
        }


        private static void RenderContentPage(CmsPageRequest rt)
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


            var compositor = new RenderFragmentCompositor(pageComposition, batch);

            var writer = new HtmlOnlyComposedHtmlWriter();
            compositor.WriteComposedFragments(fragmentMode, writer);

            HttpContext.Current.Response.Write(writer.ToString());
        }

        public bool IsReusable { get; } = false;
    }
}
