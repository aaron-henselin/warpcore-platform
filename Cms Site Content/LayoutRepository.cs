using System;
using System.Collections.Generic;
using WarpCore.Cms;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Orm;

namespace Cms.Layout
{
    [Table("cms_page_layout")]
    [WarpCoreEntity(ApiId, TitleProperty = nameof(Name), ContentNameSingular = "Page Layout")]
    public class Layout : UnversionedContentEntity
    {
        private const string ApiId = "4b6a87ff-002e-4897-82e9-e37b7b42a497";

        public string Name { get; set; }
        public string MasterPagePath { get; set; }

        public Guid? ParentLayoutId { get; set; }

        [SerializedComplexObject]
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();


    }

    public class LayoutNode
    {
        public LayoutNode ParentNode { get; set; }
        public Layout Layout { get; set; }
    }

    [ExposeToWarpCoreApi(ApiId)]
    public class LayoutRepository : UnversionedContentRepository<Layout>
    {
        public const string ApiId = "4e3e0fdb-5008-4239-93cc-3ac6307e2a5d";

        public LayoutNode GetLayoutStructure(Layout layout)
        {
            LayoutNode ln = null;
            if (layout.ParentLayoutId != null)
            {
                var parentLayout = GetById(layout.ParentLayoutId.Value);
                ln = GetLayoutStructure(parentLayout);
            }

            return new LayoutNode
            {
                Layout = layout,
                ParentNode = ln
            };
        }
    }
}
