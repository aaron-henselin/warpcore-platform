using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using WarpCore.Cms.Routing;
using WarpCore.Platform.Kernel;

namespace Platform_WebPipeline
{
    public interface IRouteData
    {
        IDictionary<string,object> DataTokens { get; }
    }

    public interface IHttpRequest
    {
        NameValueCollection QueryString { get; }

        Uri Uri { get; }
    }

    public static class HttpRequestExtensions
    {
        public static UriBuilderContext ToUriBuilderContext(this IHttpRequest request)
        {
            var url = request.Uri;
            return new UriBuilderContext()
            {
                Authority = url.Authority,
                AbsolutePath = new Uri(url.AbsolutePath, UriKind.Relative),
                IsSsl = url.Scheme == "https"
            };
        }

    }


    public interface IWebServer
    {
        string MapPath(string virtualPath);

    }

    public interface IPerRequestItems
    {
         T Get<T>(string key);
         void Set<T>(string key, T value);
    }

    public static class WebDependencies
    {
        public static IHttpRequest Request => Dependency.Resolve<IHttpRequest>();
        public static IWebServer WebServer => Dependency.Resolve<IWebServer>();
        public static IPerRequestItems Items => Dependency.Resolve<IPerRequestItems>();
        public static IRouteData RouteData => Dependency.Resolve<IRouteData>();
        
    }



}
