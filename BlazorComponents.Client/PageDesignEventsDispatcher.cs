using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorComponents.Shared;

namespace BlazorComponents.Client
{
    public class FormEventDispatcher
    {
    }

    public class PageDesignEventsDispatcher
    {
        public Action<Guid> Edit { get; set; }

        public Action<Guid> Delete { get; set; }
    }

    public class DesignerChrome
    {
        public SideBarMode SideBarMode { get; set; }

        public static DesignerChrome Default => new DesignerChrome();

    }

    public enum SideBarMode { Collapsed, Toolbox, Configurator}


}
