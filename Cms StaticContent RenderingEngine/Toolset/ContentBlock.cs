using System;
using System.ComponentModel;
using WarpCore.Platform.DataAnnotations;

namespace Cms_StaticContent_RenderingEngine
{
    [global::WarpCore.Cms.Toolbox.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Content Block", Category = "Content")]
    public class ContentBlock : StaticContentControl
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


}
