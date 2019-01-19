﻿using System;
using System.Collections.Generic;
using System.Text;
using WarpCore.Platform.DataAnnotations;

namespace BlazorComponents.Shared
{


    public class ConfiguratorSetup
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public string PropertyType { get; set; }
        public Editor Editor { get; set; }
        public string CustomEditorCode { get; set; }
    }

    public class ConfiguratorRow
    {
        public int NumColumns { get; set; }
    }

}
