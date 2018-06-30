using System;
using System.ComponentModel.DataAnnotations.Schema;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms.Sites
{

    public class SiteRepository : UnversionedContentRepository<Site>
    {



    }



    [Unversioned]
    [Table("cms_site")]
    public class Site : UnversionedContentEntity
    {
        public string Name { get; set; }
        public string RoutePrefix { get; set; }
        public string UriAuthority { get; set; } = UriAuthorityFilter.Any;
        public int Priority { get; set; }

        public Guid? HomepageId { get; set; }
    }

    public struct UriAuthorityFilter
    {
        public static string Any => "*";
    }



}
