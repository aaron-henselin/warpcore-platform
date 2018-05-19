using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarpCore.Cms
{
    public enum PageType { ContentPage,GroupingPage,Redirect}

    [Table("cms_page")]
    public class Page
    {
        [Column]
        public string Name { get; set; }

        [Column]
        public Guid? ParentPageId { get; set; }

        [Column]
        public Guid SiteId { get; set; }

        [Column]
        public List<PageContent> PageContent { get; set; } = new List<PageContent>();
    }

    public class PageContent
    {
    }


}
