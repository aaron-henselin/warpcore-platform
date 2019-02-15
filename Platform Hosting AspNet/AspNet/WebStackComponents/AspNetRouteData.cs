using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Platform_WebPipeline;
using WarpCore.Platform.Kernel;

namespace Platform_Hosting_AspNet.AspNet
{
    public class AspNetRouteData : IRouteData
    {
        public IDictionary<string, object> DataTokens =>
            HttpContext.Current.Request.RequestContext.RouteData.DataTokens;
    }
}
