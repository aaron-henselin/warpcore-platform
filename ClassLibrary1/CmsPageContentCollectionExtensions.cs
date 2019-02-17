using System.Collections.Generic;
using Cms.Forms;
using Cms_StaticContent_RenderingEngine;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;

namespace Cms
{
    public static class CmsPageContentCollectionExtensions
    {
        public static void AddDynamicForm(this ICollection<CmsPageContent> allContent, CmsForm form, string contentPageHolderId ="Body")
        {
            allContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = contentPageHolderId,
                WidgetTypeCode = "wc-dynamic-form",
                Parameters = new Dictionary<string, string> { ["FormId"] = form.ContentId.ToString() }
            });
        }

        public static void AddBlazorApp(this ICollection<CmsPageContent> allContent, BlazorApp app, string contentPageHolderId = "Body")
        {
            var cmsPageContent = new InstallationHelpers().BuildCmsPageContentFromToolboxItemTemplate(app);
            cmsPageContent.PlacementContentPlaceHolderId = contentPageHolderId;
            allContent.Add(cmsPageContent);
        }

    }
}