using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.DbEngines.AzureStorage;

namespace Cms.Layout
{
    [Table("cms_layout")]
    public class Layout : UnversionedContentEntity
    {
        public string MasterPagePath { get; set; }
    }

    public class LayoutRepository : UnversionedContentRepository<Layout>
    {

    }
}
