using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WarpCore.Cms.Routing;

namespace WarpCore.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static UriBuilderContext ToUriBuilderContext(this HttpContext context)
        {
            var url = context.Request.Url;
            return new UriBuilderContext()
            {
                Authority = url.Authority,
                AbsolutePath = url.AbsolutePath,
                IsSsl = url.Scheme == "https"
            };
        }
    }
}