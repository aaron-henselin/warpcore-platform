using System;
using System.Collections.Generic;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.DataAnnotations;

namespace WarpCore.Web.Widgets
{
    public interface ILayoutControl
    {
        IReadOnlyCollection<string> InitializeLayout();

        IReadOnlyCollection<PageCompositionElement> GetAutoIncludedElementsForPlaceHolder(string placeHolderId);

        [UserInterfaceIgnore]
        Guid LayoutBuilderId { get;  }
    }



}