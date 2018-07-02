using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms.Sites
{

    public class SiteRepository : UnversionedContentRepository<Site>
    {



    }


    
    [Table("cms_site")]
    public class Site : UnversionedContentEntity
    {
        public string Name { get; set; }
        public string RoutePrefix { get; set; }
        public string UriAuthority { get; set; } = UriAuthorityFilter.Any;
        public int Priority { get; set; }
        public Guid? HomepageId { get; set; }
    }

    [Table("cms_search_index")]
    public class SearchIndex : UnversionedContentEntity
    {
        public string Name { get; set; }
        public List<SearchIndexInclude> SearchIndexIncludes { get; set; } = new List<SearchIndexInclude>();
    }

    public class SearchIndexInclude
    {
    }

    public struct UriAuthorityFilter
    {
        public static string Any => "*";
    }



}
