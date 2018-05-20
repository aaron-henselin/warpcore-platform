using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WarpCore.Cms
{

    [Table("cms_site")]
    public class Site
    {
        [Column]
        public Guid Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string RoutePrefix { get; set; }

        [Column]
        public string UriAuthority { get; set; }

        [Column]
        public int Priority { get; set; }

        [Column]
        public Guid? HomepagePageId { get; set; }
    }

    public class SiteRepository
    {
        public IEnumerable<Site> GetAllSites()
        {
            return new List<Site>();
        }
    }
}
