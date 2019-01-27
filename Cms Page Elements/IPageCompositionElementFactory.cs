using System;
using System.Collections.Generic;

namespace Modules.Cms.Features.Presentation.Page.Elements
{

    public interface IPageCompositionElementFactory
    {
        Type Build(string virtualPath);
        object ActivateType(Type type);
        IReadOnlyCollection<Type> GetHandledBaseTypes();
        IReadOnlyCollection<string> GetHandledFileExtensions();

        PageCompositionElement CreateRenderingForObject(object nativeWidgetObject);
        
    }
}