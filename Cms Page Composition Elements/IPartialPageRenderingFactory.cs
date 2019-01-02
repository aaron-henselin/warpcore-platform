using System;
using System.Collections.Generic;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace WarpCore.Web
{
    public interface IPartialPageRenderingFactory
    {
        object ActivateType(Type type);

        IReadOnlyCollection<Type> GetHandledBaseTypes();

        IReadOnlyCollection<string> GetHandledFileExtensions();

        PageCompositionElement CreateRenderingForObject(object nativeWidgetObject);

        PageCompositionElement CreateRenderingForPhysicalFile(string physicalFilePath);
    }
}