using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Modules.Cms.Features.Presentation.Cache;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Kernel;

namespace Cms_StaticContent_RenderingEngine
{
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
    public class BlazorApp : StaticContentControl, ISupportsCache<ByInstance>
    {
        public const string ApiId = "warpcore-blazor-app";

        [UserInterfaceHint(Editor = Editor.RichText)]
        [DisplayName("App")]
        public string AppName { get; set; }

        public override StaticContent GetStaticContent()
        {
            var currentUri = Dependency.Resolve<IHttpRequest>().Uri;
            var absPath = currentUri.AbsolutePath;
            if (!absPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                absPath += "/";

            var globalContent = new Dictionary<string,string>();
            globalContent.Add(GlobalLayoutPlaceHolderIds.Head, $"<base href='{WebUtility.HtmlEncode(absPath)}'/>");
            globalContent.Add(GlobalLayoutPlaceHolderIds.Scripts, @"<script src = './_framework/blazor.webassembly.js' ></script>");

            return new StaticContent
            {
                Html = @"<app>Loading...</app>",
                GlobalContent = globalContent
            };
        }


        


    }

}
