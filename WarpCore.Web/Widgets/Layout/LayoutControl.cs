﻿using System;
using System.Web.UI;
using Cms.Toolbox;

namespace WarpCore.Web.Widgets
{
    public abstract class LayoutControl :Control
    {
        public abstract void InitializeLayout();

        [DesignIgnore]
        public Guid LayoutBuilderId { get; set; }
    }
}