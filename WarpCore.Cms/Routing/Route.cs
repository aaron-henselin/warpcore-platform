using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarpCore.Cms.Routing
{
    public enum RoutePriority
    {
        Primary = 0,
        Former = 10,
    }

    [Table("cms_route")]
    public class Route
    {
        [Column]
        public string SitemapPath { get; set; }

        [Column]
        public int Priority { get; set; }

        [Column]
        public Guid PageId { get; set; }

        [Column]
        public Guid? ContentId { get; set; }

        [Column]
        public string ContentTypeCode{ get; set; }
    }

    [Table("cms_site")]
    public class Site
    {
        [Column]
        public string Name { get; set; }

        [Column]
        public string RoutePrefix { get; set; }

        [Column]
        public string UriAuthority { get; set; }

        [Column]
        public int Priority { get; set; }

        
    }

    public class SiteRepository
    {
        public IEnumerable<Site> GetAllSites()
        {
            return new List<Site>();
        }
    }

    public class RouteRepository
    {
        public IEnumerable<Route> GetAllRoutes()
        {
            return new List<Route>();
        }
    }
}