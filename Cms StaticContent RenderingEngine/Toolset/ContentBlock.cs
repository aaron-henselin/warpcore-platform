using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using BlazorComponents.Shared;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform_WebPipeline;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.UserInteraceHints;
using WarpCore.Platform.Extensibility;

namespace Cms_StaticContent_RenderingEngine
{
    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Configurator Row", Category = "Form")]
    public class ConfiguratorRow : StaticContentControl, ISupportsCache<ByParameters>
    {
        public const string ApiId = "ConfiguratorRow";

        [UserInterfaceHint]
        [DisplayName("Number of Columns")]
        public int NumColumns { get; set; }

        public override StaticContent GetStaticContent()
        {
            return new StaticContent { Html = string.Empty };
        }
    }

    //[global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Configurator Element", Category = "Form")]
    //public class ConfiguratorElement : StaticContentControl, ISupportsCache<ByParameters>
    //{
    //    public const string ApiId = "ConfiguratorElement";

    //    [UserInterfaceHint]
    //    [DisplayName("Property Name")]
    //    public string PropertyName { get; set; }

    //    [UserInterfaceHint]
    //    [DisplayName("Display Name")]
    //    public string DisplayName { get; set; }

    //    [UserInterfaceHint]
    //    [DisplayName("Property Type")]
    //    public string PropertyType { get; set; }

    //    [UserInterfaceHint]
    //    [DisplayName("Editor Code")]
    //    public string EditorCode { get; set; }

    //    public override StaticContent GetStaticContent()
    //    {
    //        return new StaticContent { Html = string.Empty };
    //    }
    //}


    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Block", Category = "Content")]
    public class ContentBlock : StaticContentControl,ISupportsCache<ByParameters>
    {
        public const string ApiId = "warpcore-content-html";

        [UserInterfaceHint(Editor = Editor.RichText)]
        [DisplayName("Html")]
        public string AdHocHtml { get; set; }

        public override StaticContent GetStaticContent()
        {
            return new StaticContent{Html=AdHocHtml};
        }

    }

    public class RouteDataDictionary : Dictionary<string, string>, ISupportsJavaScriptSerializer
    {
        public static RouteDataDictionary Empty => new RouteDataDictionary();
    }




    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Browser Launcher", Category = "Data")]
    public class ContentBrowserApp : BlazorApp
    {
        public const string ApiId = "wc-content-browser-launcher";



        [UserInterfaceHint(Editor = Editor.SubForm)]
        public ContentBrowserConfiguration Configuration { get; set; } = new ContentBrowserConfiguration();


        public ContentBrowserApp() : base("content/{RepositoryApiId}")
        {
        }

        protected override RouteDataDictionary GetStartingRouteParameters()
        {
            return new RouteDataDictionary {{"RepositoryApiId", Configuration.RepositoryApiId.ToString()}};
        }
    }


    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Browser Launcher", Category = "Data")]
    public class PageTreeApp : BlazorApp
    {
        public const string ApiId = "wc-page-tree-browser";

        public PageTreeApp() : base("pages")
        {
        }

        protected override RouteDataDictionary GetStartingRouteParameters()
        {
            return RouteDataDictionary.Empty;
            
        }
    }

    

    public abstract class BlazorApp : StaticContentControl, ISupportsCache<ByInstance>, IHostsClientSideRoutes
    {
        public BlazorApp(string appRouteTemplate)
        {
        }
        
        protected string StartingRouteTemplate { get; set; }
        protected abstract RouteDataDictionary GetStartingRouteParameters();
        protected abstract object GetApplicationConfiguration();


        public override StaticContent GetStaticContent()
        {
            //route here is the _app host_
            var applicationBaseUri = WebPipeline.CurrentRequest.Route.VirtualPath;
            var absPath = applicationBaseUri.ToString();
            if (!absPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                absPath += "/";

            var baseTag = $"<base href='{WebUtility.HtmlEncode(absPath)}'/>";
            var stylesheetTag = $"<link rel='stylesheet' type='text/css' href='./_framework/app.css'/>";

            var startingRoute = StartingRouteTemplate;
            var startingRouteParameters = GetStartingRouteParameters();
            foreach (var kvp in startingRouteParameters)
                startingRoute = startingRoute.Replace("{" + kvp.Key + "}", WebUtility.UrlEncode(kvp.Value));

            var globalContent = new Dictionary<string,string>();
            globalContent.Add(GlobalLayoutPlaceHolderIds.Head, baseTag+stylesheetTag);

            var wrapper = new
            {
                StartPage = startingRoute,
                Configuration = GetApplicationConfiguration(),
            };

            var scriptLine1 = $@"window.__blazorInstanceData = {JsonConvert.SerializeObject(wrapper)};";
            var scriptLine2 = "window.__getBlazorStartPage = function(){ return window.__blazorInstanceData.StartPage; };";
            var scriptLine3 = "window.__getBlazorAppConfig = function(){ return JSON.stringify(window.__blazorInstanceData.Configuration); };";

            var scriptTag1 = $@"<script>
                                     {scriptLine1}   
                                     {scriptLine2}   
                                     {scriptLine3} 
                              </script>";
            var scriptTag2 = "<script src='./_framework/blazor.webassembly.js'></script>";



            globalContent.Add(GlobalLayoutPlaceHolderIds.Scripts,scriptTag1 + scriptTag2);

            return new StaticContent
            {
                Html = $@"<app>Loading...</app>",//todo: encode
                GlobalContent = globalContent
            };
        }


        


    }

}
