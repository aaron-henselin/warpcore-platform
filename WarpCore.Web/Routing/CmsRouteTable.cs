using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.Cms
{
    public class CmsRouteTable
    {
        private readonly Dictionary<Guid,CmsSiteRouteTable> _siteRouteTables = new Dictionary<Guid, CmsSiteRouteTable>();

        public CmsRouteTable()
        {
            var allRoutes = RouteBuilder.DiscoverRoutes();
            var bySiteId = allRoutes.ToLookup(x => x.SiteId);
            foreach (var grouping in bySiteId)
            {
                var subRouteTable = new CmsSiteRouteTable(grouping.ToList());
                _siteRouteTables.Add(grouping.Key,subRouteTable);
            }
        }

        public bool TryGetRoute(Uri absoluteUri, out SiteRoute route)
        {
            route = null;

            var allSites = new SiteRepository().GetAllSites().ToList();
            var site = allSites.SingleOrDefault(x => x.UriAuthority == absoluteUri.Authority);
            if (site == null)
                site = allSites.SingleOrDefault(x => string.IsNullOrWhiteSpace(x.UriAuthority));

            if (site == null)
                return false;

            var absPath = new Uri(absoluteUri.AbsolutePath, UriKind.Relative);
            return _siteRouteTables[site.Id].TryGetRoute(absPath, out route);
        }

        public static CmsRouteTable Current { get; set; } = new CmsRouteTable();
    }
}