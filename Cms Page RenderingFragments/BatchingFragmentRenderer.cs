using System.Collections.Generic;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public class BatchingFragmentRenderer
    {
        private readonly IEnumerable<IFragmentRenderer> _engines;

        public BatchingFragmentRenderer() : this(Dependency.ResolveMultiple<IFragmentRenderer>())
        {
        }
        public BatchingFragmentRenderer(IEnumerable<IFragmentRenderer> engines)
        {
            _engines = engines;
        }


        public RenderingFragmentCollection Execute(Features.Presentation.PageComposition.Elements.PageComposition pageRendering,FragmentRenderMode renderMode)
        {
            var transformationResult = new RenderingFragmentCollection();

            foreach (var engine in _engines)
            {
                var localBatch = engine.Execute(pageRendering.RootElement);
                transformationResult.Add(localBatch);
            }

            return transformationResult;


            //var allDescendents = pageRendering.Rendering.GetAllDescendents();

            //var literals = allDescendents.OfType<LiteralPartialPageRendering>().ToList();
            //foreach (var literal in literals)
            //{
            //    var htmlOutput = new HtmlOutput(new StringBuilder(literal.Text));
            //    batch.WidgetContent.Add(literal.ContentId,new List<IRenderingFragment> {htmlOutput});
            //}



        }


    }
}