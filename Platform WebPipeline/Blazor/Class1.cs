using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Platform_WebPipeline.Requests;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;
using WarpCore.Web;

namespace Platform_WebPipeline
{


    public class WarpCoreBlazorActionBuilder
    {
        private readonly IHttpRequest httpRequest;

        public WarpCoreBlazorActionBuilder(IHttpRequest httpContext)
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
                var vPath = new BlazorToolkit().GetHostedPath(key);
                //HttpContext.Current.RewritePath(vPath);
                return new RewriteUrl(vPath);
            }

            return null;
        }
    }

    public class BlazorToolkit
    {
        static Dictionary<string, string> _hostedFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string GetHostedPath(string key)
        {
            return _hostedFiles[key];
        }

        public IReadOnlyCollection<string> FindIncludedBlazorModules(Assembly asm)
        {
            List<string> allLibraries = new List<string>();

            var resources = asm.GetManifestResourceNames();
            var booters = resources.Where(x => x.EndsWith(".blazor.boot.json", StringComparison.OrdinalIgnoreCase));
            foreach (var bootFile in booters)
            {
                var contents = AssemblyResourceReader.ReadString(asm, bootFile);
                var jObject = JObject.Parse(contents);
                var libraryName = jObject["main"].ToString();

                if (libraryName.EndsWith(".dll"))
                {
                    libraryName = libraryName.Substring(0, libraryName.Length - 4);
                    allLibraries.Add(libraryName);
                }
            }

            return allLibraries;
        }

        private Dictionary<string, byte[]> FindIncludedBlazorModuleFiles(Assembly asm)
        {
            var generatedVpp = new Dictionary<string, byte[]>();

            var libraries = FindIncludedBlazorModules(asm);
            var allResources = asm.GetManifestResourceNames();

            foreach (var libraryName in libraries)
            {
                string cssMerged = string.Empty;
                var cssSearch = libraryName + ".dist.css.";
                var cssResources = allResources.Where(r => r.Contains(cssSearch));
                foreach (var resource in cssResources)
                {
                    cssMerged += Environment.NewLine;
                    cssMerged += $"/* {resource} */";
                    cssMerged += Environment.NewLine;
                    cssMerged += AssemblyResourceReader.ReadString(asm, resource);

                }
                var appCss = libraryName + "/dist/_framework/app.css";
                generatedVpp.Add(appCss, Encoding.UTF8.GetBytes(cssMerged));

                var binSearch = libraryName + ".dist._framework._bin.";
                var heuristicallyFoundResources = allResources.Where(r => r.Contains(binSearch));

                string prefix = null;
                foreach (var resourcesToInclude in heuristicallyFoundResources)
                {
                    var at = resourcesToInclude.IndexOf(binSearch, StringComparison.OrdinalIgnoreCase);
                    prefix = resourcesToInclude.Substring(0, at - 1);

                    var pathRight = resourcesToInclude.Substring(at + binSearch.Length);
                    var fullPath = libraryName + "/dist/_framework/_bin/" + pathRight;
                    var resourceContents = AssemblyResourceReader.ReadBinary(asm, resourcesToInclude);
                    generatedVpp.Add(fullPath, resourceContents);
                }

                var monoJs = libraryName + "/dist/_framework/wasm/mono.js";
                var monoWasm = libraryName + "/dist/_framework/wasm/mono.wasm";
                var blazor_boot = libraryName + "/dist/_framework/blazor.boot.json";
                var blazor_server = libraryName + "/dist/_framework/blazor.server.js";
                var blazor_webassembly = libraryName + "/dist/_framework/blazor.webassembly.js";

                foreach (var resourceToInclude in new[]
                    {monoJs, monoWasm, blazor_boot, blazor_server, blazor_webassembly})
                {
                    generatedVpp.Add(resourceToInclude, AssemblyResourceReader.ReadBinary(asm, prefix + "." + resourceToInclude.Replace("/", ".")));

                }


            }

            return generatedVpp;
        }

        public void HostBlazorModule(Assembly assembly)
        {
            var webServer = Dependency.Resolve<IWebServer>();

            var allFiles = FindIncludedBlazorModuleFiles(assembly);
            var appData = "~/App_Data/WarpCore/Temp/BlazorModules/";
            foreach (var fileToWrite in allFiles)
            {
                var fullVpath = (appData + fileToWrite.Key);

                var lastDirIndex = fullVpath.LastIndexOf("/");
                var directoryToCreateVirtual = fullVpath.Substring(0, lastDirIndex);
                var directoryToCreatePhysical =webServer.MapPath(directoryToCreateVirtual);
                Directory.CreateDirectory(directoryToCreatePhysical);


                var fileToCreatePhysical = webServer.MapPath(fullVpath);

                int tryCount = 0;
                bool written = false;
                while (!written)
                    try
                    {
                        File.WriteAllBytes(fileToCreatePhysical, fileToWrite.Value);
                        written = true;
                    }
                    catch (Exception e)
                    {

                        tryCount++;
                        if (tryCount < 5)
                            Thread.Sleep(1000);
                        else
                        {
                            throw;
                        }
                    }


                _hostedFiles.Add(fileToWrite.Key, fullVpath);
            }

        }

    }

    public class WarpCorePageRequestActionBuilder
    {
        private readonly IHttpRequest httpRequest;

        public WarpCorePageRequestActionBuilder(IHttpRequest httpContext)
        {
            this.httpRequest = httpContext;
        }

        private static WebPipelineAction ProcessRequestForContentPage(CmsPage cmsPage)
        {
            string transferUrl;

            if (!string.IsNullOrWhiteSpace(cmsPage.PhysicalFile))
            {
                transferUrl = cmsPage.PhysicalFile;
                return new RewriteUrl(transferUrl);
            }
            else
            {
                return new RenderPage();
            }


        }


        private string CreateUrl(SiteRoute transferRoute, IDictionary<string, string> parameters)
        {

            var uriBuilder = new CmsUriBuilder(httpRequest.ToUriBuilderContext());
            return uriBuilder.CreateUriForRoute(transferRoute, UriSettings.Default, parameters).ToString();

        }

        public WebPipelineAction ProcessRequest(CmsPageRequest pageRequest)
        {
            //var siteRoute = (SiteRoute)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.RouteDataToken];
            //var contentEnvironment = (ContentEnvironment)context.Request.RequestContext.RouteData.DataTokens[CmsRouteDataTokens.ContentEnvironmentToken];

            var siteRoute = pageRequest.Route;

            if (siteRoute is RedirectPageRoute)
            {
                var redirectRoute = (RedirectPageRoute)siteRoute;
                if (redirectRoute.InternalRedirectPageId == null)
                    return new UnhandledRequest();

                return ProcessRedirectPageRequest(redirectRoute);
            }


            if (siteRoute is GroupingPageRoute)
            {
                var redirectRoute = (GroupingPageRoute)siteRoute;
                if (redirectRoute.InternalRedirectPageId == null)
                    return new UnhandledRequest();

                return ProcessGroupingPageRequest(redirectRoute);
            }


            return ProcessRequestForContentPage(pageRequest.CmsPage);

        }

        private Redirect ProcessGroupingPageRequest(GroupingPageRoute redirectRoute)
        {

            SiteRoute redirectToRoute;
            CmsRoutes.Current.TryResolveRoute(redirectRoute.InternalRedirectPageId.Value, out redirectToRoute);

            var redirectUrl = CreateUrl(redirectToRoute, null);
            return new Redirect(redirectUrl);
        }

        private Redirect ProcessRedirectPageRequest(RedirectPageRoute redirectRoute)
        {
            if (!string.IsNullOrWhiteSpace(redirectRoute.RedirectExternalUrl))
            {
                return new Redirect(redirectRoute.RedirectExternalUrl);
            }

            SiteRoute redirectToRoute;
            CmsRoutes.Current.TryResolveRoute(redirectRoute.InternalRedirectPageId.Value, out redirectToRoute);

            var redirectUrl = CreateUrl(redirectToRoute, redirectRoute.InternalRedirectParameters);
            return new Redirect(redirectRoute.RedirectExternalUrl);

        }
    }
}
