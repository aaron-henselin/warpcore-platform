using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorComponents.Shared;

namespace BlazorComponents.Client
{


    public class ContentLocation
    {
        public PreviewNode ParentPlaceHolder { get; set; }
        public PreviewNode ParentWidget { get; set; }
    }

    public class PagePreviewPosition
    {
        public Guid ToChildOf { get; set; }
        public Guid? PlaceAfter { get; set; }
    }
}
