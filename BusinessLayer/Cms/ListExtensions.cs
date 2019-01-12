using System.Collections.Generic;
using Cms.Forms;
using WarpCore.Cms;

namespace Cms
{
    public static class ListExtensions
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


    }
}