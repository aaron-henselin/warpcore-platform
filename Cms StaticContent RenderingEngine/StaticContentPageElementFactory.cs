using System;
using System.Collections.Generic;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.Kernel;

namespace Cms_StaticContent_RenderingEngine
{
    public class StaticContentPageElementFactory : IPageCompositionElementFactory
    {
        public Type Build(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public object ActivateType(Type type)
        {
            return Dependency.Resolve(type);
        }

        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new []{typeof(StaticContentControl)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new string[0];
        }

        public PageCompositionElement CreateRenderingForObject(object nativeWidgetObject)
        {
            var ctrl = (StaticContentControl) nativeWidgetObject;
            var staticContent = ctrl.GetStaticContent();
            return new StaticContentPageElement(staticContent);
        }
    }
}