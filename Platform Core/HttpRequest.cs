using System.Collections.Specialized;

namespace WarpCore.Platform.Kernel
{
    public interface IHttpRequest
    {
        NameValueCollection QueryString { get; }
    }

    public interface IWebServer
    {
        string MapPath(string virtualPath);
    }

    public interface IHttpItems
    {
         T Get<T>(string key);
         void Set<T>(string key, T value);
    }
}
