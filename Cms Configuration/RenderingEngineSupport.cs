using System;
using System.Web.Http;

namespace Modules.Cms.Features.Configuration
{
    public class RenderingEngineSupport
    {
        public Type PageCompositionFactory;
        public Type FragmentRenderer;
        public Action<HttpConfiguration> SetupMethod;
    }
}