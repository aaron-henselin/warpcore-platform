namespace Platform_WebPipeline
{
    public class BlazorWebPipeline
    {
        private readonly IHttpRequest httpRequest;

        public BlazorWebPipeline(IHttpRequest httpContext)
        {
            this.httpRequest = httpContext;
        }

        public WebPipelineAction TryProcessAsBlazorRequest()
        {


            //todo: figure out what the hell to do with this.. can we make this logic dependent on the requesting app?
            //var currentUri = httpRequest.Uri.ToString();//httpRequest.RawUrl;
            //if (currentUri.Contains("/_framework/"))
            if (httpRequest.Uri.AbsolutePath == "/BlazorResource")
            {
                //var indexOf = currentUri.IndexOf("/_framework/");
                //var right = currentUri.Substring(indexOf + 1);
                var appPath = System.Web.HttpUtility.ParseQueryString(httpRequest.Uri.Query)["path"];


                var key = $"BlazorComponents.Client/dist/_framework/{appPath}";
                var vPath = new BlazorModuleBuilder().GetHostedPath(key);
                //HttpContext.Current.RewritePath(vPath);
                return new RewriteUrl(vPath);
            }

            return null;
        }
    }
}