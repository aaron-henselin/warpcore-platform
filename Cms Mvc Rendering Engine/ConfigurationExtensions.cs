using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
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



        //public class MyAssembliesResolver : DefaultAssembliesResolver
        //{
        //    public override ICollection<Assembly> GetAssemblies()
        //    {
        //        return AppDomain.CurrentDomain.GetAssemblies();
        //    }
        //}
    }
}
