using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WarpCore.Cms
{

    [Table("cms_site")]
    public class Site
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RoutePrefix { get; set; }
        public string UriAuthority { get; set; }
        public int Priority { get; set; }
    }

    public class SiteRepository
    {
        public IEnumerable<Site> GetAllSites()
        {
            return new List<Site>();
        }
    }
}
