using System.Collections.Generic;

namespace Modules.Cms.Features.Configuration
{
    public class CmsConfiguration
    {
        private readonly IList<RenderingEngineSupport> _supportedEngines = new List<RenderingEngineSupport>();

        public void AddRenderingEngineSupport(RenderingEngineSupport renderingSupport)
        {
            _supportedEngines.Add(renderingSupport);
        }

        public IEnumerable<RenderingEngineSupport> SupportedRenderingEngines => _supportedEngines;

       public static CmsConfiguration Current { get; set; } = new CmsConfiguration();
    }




}