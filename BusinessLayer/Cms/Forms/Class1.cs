using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.Cms;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.Forms
{
    [Table("cms_form")]
    public class CmsForm : VersionedContentEntity, IDesignable
    {
        [Column]
        public string Name { get; set; }

        [StoreAsComplexData]
        public List<CmsPageContent> FormContent { get; set; } = new List<CmsPageContent>();

        public List<CmsPageContent> DesignedContent => FormContent;
        public Guid DesignForContentId => ContentId.Value;
    }

    public class FormRepository : VersionedContentRepository<CmsForm>
    {

    }

}
