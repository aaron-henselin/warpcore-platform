using System.Collections.Generic;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.RenderingEngines.CachedContent;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
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


        public RenderingFragmentCollection Execute(Page.Elements.PageComposition pageRendering,FragmentRenderMode renderMode)
        {
            var transformationResult = new RenderingFragmentCollection();

            var cached =  new CachedContentFragmentRenderer().Execute(pageRendering.RootElement);
            transformationResult.Add(cached);

            foreach (var engine in _engines)
            {
                var localBatch = engine.Execute(pageRendering.RootElement);
                transformationResult.Add(localBatch);
            }

            return transformationResult;
        }


    }
}