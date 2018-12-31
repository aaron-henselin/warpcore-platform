using System;
using System.Collections.Generic;
using System.Web.UI;
using Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;

namespace WarpCore.Web.Widgets
{
    public interface ILayoutControl
    {
        IReadOnlyCollection<RenderingsPlaceHolder> InitializeLayout();

        [UserInterfaceIgnore]
        Guid LayoutBuilderId { get; set; }
    }


}