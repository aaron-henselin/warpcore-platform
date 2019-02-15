using Platform_WebPipeline;
using WarpCore.Platform.Kernel;
using HttpContext = System.Web.HttpContext;

namespace Platform_Hosting_AspNet
{

    public class AspNetItems : IPerRequestItems
    {
        public T Get<T>(string key)
        {
            return (T)HttpContext.Current.Items[key];
        }

        public void Set<T>(string key, T value)
        {
            HttpContext.Current.Items[key] = value;
        }
    }
}
