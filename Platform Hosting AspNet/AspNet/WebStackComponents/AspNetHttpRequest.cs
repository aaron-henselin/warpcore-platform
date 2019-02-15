using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Platform_WebPipeline;
using WarpCore.Platform.Kernel;
using HttpContext = System.Web.HttpContext;

namespace Platform_Hosting_AspNet
{
    public class AspNetHttpRequest : IHttpRequest
    {
        public AspNetHttpRequest()
        {
           
        }

        public NameValueCollection QueryString => HttpContext.Current.Request.QueryString;

        public Uri Uri => HttpContext.Current.Request.Url;


    }
}
