using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.Cms
{
    public static class CmsRoutes
    {
        private static CmsRouteTable _current;

        public static CmsRouteTable Current
        {
            get
            {
                if (_current == null)
                    _current = CreateRouteTable();

                return _current;
            }
        }

        private static CmsRouteTable CreateRouteTable()
        {
            var rt = new CmsRouteTable();
            var allSites = new SiteRepository().Find().ToList();
            foreach (var site in allSites)
            {
                var allRoutes = RouteBuilder.DiscoverRoutesForSite(site).ToList();
                var subRouteTable = new CmsSiteRouteTable(allRoutes);
                rt.AddSubTable(site,subRouteTable);
            }

            return rt;
        }
    }

    public class CmsRouteTable
    {

        private readonly Dictionary<string,CmsSiteRouteTable> _siteRouteTables = new Dictionary<string, CmsSiteRouteTable>();
       

        public void AddSubTable(Site site, CmsSiteRouteTable subRouteTable)
        {
            _siteRouteTables.Add(site.UriAuthority, subRouteTable);

        }

        public bool TryResolveRoute(Guid pageId, out SiteRoute route)
        {
            route = null;

            foreach (var site in _siteRouteTables.Keys)
            {
                var success = _siteRouteTables[site].TryGetRoute(pageId, out route);
                if (success)
                    return true;
            }

            return false;
        }

        public bool TryResolveRoute(Uri absoluteUri, out SiteRoute route)
        {
            route = null;

            var site = GetCurrentSite(absoluteUri);

            if (site == null)
                return false;

            var absPath = new Uri(absoluteUri.AbsolutePath, UriKind.Relative);
            return _siteRouteTables[site].TryGetRoute(absPath, out route);
        }

        private string GetCurrentSite(Uri absoluteUri)
        {
            var allSites = _siteRouteTables.Keys;
            var site = allSites.SingleOrDefault(x => x == absoluteUri.Authority);
            if (site == null)
                site = allSites.SingleOrDefault(string.IsNullOrWhiteSpace);
            return site;
        }

    }
}