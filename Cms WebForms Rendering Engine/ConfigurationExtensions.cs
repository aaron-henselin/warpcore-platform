using Modules.Cms.Features.Configuration;

namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{
    public static class ConfigurationExtensions
    {
        public static void AddWebFormsSupport(this CmsConfiguration cmsConfiguration)
        {
            cmsConfiguration.AddRenderingEngineSupport(new RenderingEngineSupport
            {
                FragmentRenderer = typeof(WebFormsFragmentRenderer),
                PageCompositionFactory = typeof(WebFormsPageCompositionElementFactory)
            });
        }
    }
}
