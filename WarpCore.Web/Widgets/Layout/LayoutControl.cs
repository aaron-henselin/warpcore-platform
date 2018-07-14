using System;
using System.Web.UI;

namespace WarpCore.Web.Widgets
{
    public abstract class LayoutControl :Control
    {
        public abstract void InitializeLayout();

        [Setting]
        public Guid LayoutBuilderId { get; set; } = Guid.NewGuid();
    }
}