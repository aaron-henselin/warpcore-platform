using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Context
{



    public interface ILayoutHandle
    {
        string FriendlyName { get; set; }
        string HandleName { get; set; }
        Guid PageContentId { get; set; }
    }




    public class CmsPageRequestContext
    {
        public SiteRoute Route { get; set; }
        public CmsPage CmsPage { get; set; }
        public PageRenderMode PageRenderMode { get; set; }

        public static CmsPageRequestContext Current => Dependency.Resolve<CmsPageRequestContext>();
            
          

    }

    public enum PageRenderMode { Readonly, PageDesigner }


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

    public class EditingContextManager
    {
        private JavaScriptSerializer _js;

        public EditingContextManager()
        {
            _js = new JavaScriptSerializer();
        }




        private EditingContext CreateEditingContext(IHasSubRenderingPlans hasDesignedLayout)
        {
            var ec = new EditingContext
            {
                DesignType = hasDesignedLayout.GetType().AssemblyQualifiedName,
                DesignedContentId = hasDesignedLayout.DesignForContentId,
                DesignContentTypeId = hasDesignedLayout.ContentTypeId,
                AllContent = hasDesignedLayout.DesignedContent,
            };
            var raw = _js.Serialize(ec);
            return _js.Deserialize<EditingContext>(raw);
        }

        public EditingContext GetOrCreateEditingContext(IHasSubRenderingPlans hasDesignedLayout)
        {

            var pageDesignHasNotStarted =
                HttpContext.Current.Request[EditingContextVars.SerializedPageDesignStateKey] == null;

            if (pageDesignHasNotStarted)
                HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] = CreateEditingContext(hasDesignedLayout);

            return GetEditingContext();
        }

        public EditingContext GetEditingContext()
        {
            if (HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] == null)
            {
                var json = HttpContext.Current.Request[EditingContextVars.SerializedPageDesignStateKey];
                HttpContext.Current.Items[EditingContextVars.PageDesignContextKey] = _js.Deserialize<EditingContext>(json);
            }

            return (EditingContext)HttpContext.Current.Items[EditingContextVars.PageDesignContextKey];
        }



    }

}
