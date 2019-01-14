using System;
using System.Collections.Generic;
using WarpCore.Cms;

namespace Cms_PageDesigner_Context
{

    public static class PageDesignerUriComponents
    {
        public const string PageId = "wc-pg";
        public const string SiteId = "wc-st";
        public const string ContentEnvironment = "wc-ce";
        public const string ViewMode = "wc-viewmode";
        public const string ContentVersion = "wc-cv";
    }
    public class EditingContext : IPageContent
    {
        public List<CmsPageContent> AllContent { get; set; }

        public bool IsEditing => AllContent != null;
        public Guid DesignedContentId { get; set; }
        public string DesignType { get; set; }
        public Guid DesignContentTypeId { get; set; }
    }

    public struct EditingContextVars
    {
        public const string SerializedPageDesignStateKey = "WC_EDITING_CONTEXT_JSON";
        public const string PageDesignContextKey = "WC_EDITING_CONTEXT";

        public const string ClientSideToolboxStateKey = "WC_TOOLBOX_STATE";
        public const string ClientSideConfiguratorStateKey = "WC_CONFIGURATOR_STATE";

        public const string EditingContextSubmitKey = "WC_EDITING_SUBMIT";
    }
}
