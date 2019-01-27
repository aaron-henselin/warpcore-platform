using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.Cms
{
    public class CmsSiteRouteTable
    {
        private ILookup<Uri, SiteRoute> _routesByPath;
        private ILookup<Guid, SiteRoute> _routesByPageId;

        private class AbsolutePathComparer : IEqualityComparer<Uri>
        {
            public bool Equals(Uri x, Uri y)
            {
                return string.Equals(x.ToString(),y.ToString(),StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(Uri obj)
            {
                return obj.ToString().ToLowerInvariant().GetHashCode();
            }
        }

        public CmsSiteRouteTable(IReadOnlyCollection<SiteRoute> siteRoutes)
        {
            _routesByPath = siteRoutes.ToLookup(x => x.VirtualPath,new AbsolutePathComparer());
            _routesByPageId = siteRoutes.Where(x => x.PageId != null).ToLookup(x => x.PageId.Value);
        }

        public bool TryGetRoute(Guid pageId, out SiteRoute route)
        {
            route = null;

            if (!_routesByPageId.Contains(pageId))
                return false;


            route = _routesByPageId[pageId].OrderBy(x => x.Priority).First();
            return true;
        }

        private bool FindClientSideRouteHostForVirtualPath(Uri virtualPath, out SiteRoute route)
        {
            if (_routesByPath.Contains(virtualPath))
            {
                route = _routesByPath[virtualPath].Where(x => x.HostsClientSideRoutes).OrderBy(x => x.Priority).FirstOrDefault();
                if (route != null)
                    return true;
            }


            var pathAsText = virtualPath.ToString();
            if (pathAsText.LastIndexOf("/") > 0)
            {
                
                var lastSlash = pathAsText.LastIndexOf("/");
                var newUrl = pathAsText.Substring(0, lastSlash);
                var found = FindClientSideRouteHostForVirtualPath(new Uri(newUrl, UriKind.Relative), out route);
                if (found)
                    return true;
            }

            route = null;
            return false;
        }

        public bool TryGetRoute(Uri virtualPath, out SiteRoute route)
        {

            if (_routesByPath.Contains(virtualPath))
            {
                route = _routesByPath[virtualPath].OrderBy(x => x.Priority).First();
                return true;
            }

            return FindClientSideRouteHostForVirtualPath(virtualPath, out route);
        }
    }
}