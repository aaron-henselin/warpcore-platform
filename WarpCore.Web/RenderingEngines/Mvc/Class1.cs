using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WarpCore.Web.RenderingEngines.Mvc
{
    public class MvcRenderingEngine : IPartialPageRenderingFactory
    {
        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new[] {typeof(Controller)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new[] {KnownPhysicalFileExtensions.Razor};
        }

        public PartialPageRendering CreateRenderingForObject(object nativeWidgetObject)
        {
            return new ControllerPartialPageRendering((Controller)nativeWidgetObject);
        }

        public PartialPageRendering CreateRenderingForPhysicalFile(string physicalFilePath)
        {
            throw new NotImplementedException();
        }
    }

    public class ControllerPartialPageRendering : PartialPageRendering
    {
        public ControllerPartialPageRendering(Controller controller)
        {
            //new DefaultControllerFactory().CreateController()
        }
    }
}