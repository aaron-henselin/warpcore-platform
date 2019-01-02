using System;
using System.Collections.Generic;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.DataAnnotations;

namespace WarpCore.Web.Widgets
{
    public interface ILayoutControl
    {
        void InitializeLayout();

        [UserInterfaceIgnore]
        Guid LayoutBuilderId { get; set; }
    }


}