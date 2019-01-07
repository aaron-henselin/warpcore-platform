﻿using System;
using System.Collections.Generic;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms;

namespace WarpCore.Web
{

    public interface IPageCompositionElementFactory
    {
        object ActivateType(Type type);

        IReadOnlyCollection<Type> GetHandledBaseTypes();

        IReadOnlyCollection<string> GetHandledFileExtensions();

        PageCompositionElement CreateRenderingForObject(object nativeWidgetObject);

        PageCompositionElement CreateRenderingForPhysicalFile(string physicalFilePath);
    }
}