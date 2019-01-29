using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Kernel;

namespace Cms_StaticContent_RenderingEngine
{
    [global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Configurator Row", Category = "Form")]
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

    [global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Configurator Element", Category = "Form")]
    public class ConfiguratorElement : StaticContentControl, ISupportsCache<ByParameters>
    {
        public const string ApiId = "ConfiguratorElement";

        [UserInterfaceHint]
        [DisplayName("Property Name")]
        public string PropertyName { get; set; }

        [UserInterfaceHint]
        [DisplayName("Display Name")]
        public string DisplayName { get; set; }

        [UserInterfaceHint]
        [DisplayName("Property Type")]
        public string PropertyType { get; set; }

        [UserInterfaceHint]
        [DisplayName("Editor Code")]
        public string EditorCode { get; set; }

        public override StaticContent GetStaticContent()
        {
            return new StaticContent { Html = string.Empty };
        }
    }


    [global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Block", Category = "Content")]
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


    [global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Block", Category = "Content")]
    public class BlazorApp : StaticContentControl, ISupportsCache<ByInstance>, IHostsClientSideRoutes
    {
        public const string ApiId = "warpcore-blazor-app";

        [UserInterfaceHint(Editor = Editor.RichText)]
        [DisplayName("App")]
        public string AppName { get; set; }

        public override StaticContent GetStaticContent()
        {
            //route here is the _app host_
            var applicationBaseUri = CmsPageRequestContext.Current.Route.VirtualPath;
            var absPath = applicationBaseUri.ToString();
            if (!absPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                absPath += "/";

            var baseTag = $"<base href='{WebUtility.HtmlEncode(absPath)}'/>";
            var stylesheetTag = $"<link rel='stylesheet' type='text/css' href='./_framework/app.css'/>";

            var globalContent = new Dictionary<string,string>();
            globalContent.Add(GlobalLayoutPlaceHolderIds.Head, baseTag+stylesheetTag);
            globalContent.Add(GlobalLayoutPlaceHolderIds.Scripts, @"<script src = './_framework/blazor.webassembly.js' ></script>");

            return new StaticContent
            {
                Html = @"<app>Loading...</app>",
                GlobalContent = globalContent
            };
        }


        


    }

}
