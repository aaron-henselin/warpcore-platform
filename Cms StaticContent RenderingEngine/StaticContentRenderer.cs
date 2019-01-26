using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;

namespace Cms_StaticContent_RenderingEngine
{
    public class StaticContentRenderer : IFragmentRenderer
    {
        public RenderingFragmentCollection Execute(PageCompositionElement pp)
        {
            RenderingFragmentCollection collection = new RenderingFragmentCollection();

            var allDescendents = pp.GetAllDescendents().OfType<StaticContentPageElement>().ToList();
            foreach (var item in allDescendents)
            {
                var staticContent = item.ContentControl;

                collection.RenderingResults.Add(item.ContentId,new RenderingResult
                {
                    GlobalRendering = staticContent.GlobalContent.ToDictionary(x => x.Key,x => new[]{x.Value}.ToList()),
                    InlineRenderingFragments = new List<IRenderingFragment>{new HtmlOutput(staticContent.Html)}
                });
            }

            return collection;
        }
    }
}
