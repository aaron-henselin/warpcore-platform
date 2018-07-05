using System;
using System.Web.UI;

namespace WarpCore.Web.Widgets
{
    public abstract class LayoutControl :Control
    {
        public abstract void InitializeLayout();

        public Guid LayoutBuilderId { get; set; }
    }
}