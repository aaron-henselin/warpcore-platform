using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensibility;
using WarpCore.Cms;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.Layout
{
    [Table("cms_layout")]
    public class Layout : UnversionedContentEntity
    {
        public string Name { get; set; }
        public string MasterPagePath { get; set; }

        public Guid? ParentLayoutId { get; set; }

        [SerializedComplexObject]
        public List<CmsPageContent> PageContent { get; set; } = new List<CmsPageContent>();

        public override string ToString()
        {
            return Name;
        }
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
