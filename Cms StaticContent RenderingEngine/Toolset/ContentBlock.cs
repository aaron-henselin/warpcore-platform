using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using Platform_WebPipeline;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.UserInteraceHints;

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

    }

    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Block", Category = "Content")]
    public class BlazorApp : StaticContentControl, ISupportsCache<ByInstance>, IHostsClientSideRoutes
    {
        public const string ApiId = "warpcore-blazor-app";

        [UserInterfaceHint(Editor = Editor.RichText)]
        [DisplayName("App")]
        public string StartingRouteTemplate { get; set; }

        [DisplayName("Parameters")]
        public RouteDataDictionary StartingRouteParameters { get; set; } = new RouteDataDictionary();

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
            foreach (var kvp in StartingRouteParameters)
                startingRoute = startingRoute.Replace("{" + kvp.Key + "}", WebUtility.UrlEncode(kvp.Value));

            var globalContent = new Dictionary<string,string>();
            globalContent.Add(GlobalLayoutPlaceHolderIds.Head, baseTag+stylesheetTag);
            globalContent.Add(GlobalLayoutPlaceHolderIds.Scripts, @"<script>window.__getBlazorAppStartPage = function(){ return '"+startingRoute+"';};</script>"+ $@"<script src='./_framework/blazor.webassembly.js' ></script>");

            return new StaticContent
            {
                Html = $@"<app>Loading...</app>",//todo: encode
                GlobalContent = globalContent
            };
        }


        


    }

}
