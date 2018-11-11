using System;

namespace WarpCore.Cms
{
    public class ContentRouteAttribute : Attribute
    {
        public string RouteTemplate { get; set; }
        public string ContentTypeCode { get; set; }
    }
}