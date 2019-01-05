using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Modules.Cms.Features.Configuration;
using WarpCore.Web.RenderingEngines.Mvc;

namespace Modules.Cms.Features.Presentation.RenderingEngines.Mvc
{
    public static class ConfigurationExtensions
    {
        public static void AddMvcSupport(this CmsConfiguration cmsConfiguration)
        {
            cmsConfiguration.AddRenderingEngineSupport(new RenderingEngineSupport
            {
                FragmentRenderer = typeof(MvcFragmentRenderer),
                PageCompositionFactory = typeof(MvcCompositionElementFactory)
            });

        }
    }
}
