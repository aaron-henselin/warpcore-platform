using Cms_StaticContent_RenderingEngine;
using Modules.Cms.Features.Configuration;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{
    public static class ConfigurationExtensions
    {
        public static void AddStaticContentSupport(this CmsConfiguration cmsConfiguration)
        {
            cmsConfiguration.AddRenderingEngineSupport(new RenderingEngineSupport
            {
                FragmentRenderer = typeof(StaticContentRenderer),
                PageCompositionFactory = typeof(StaticContentPageElementFactory)
            });
        }
    }
}
