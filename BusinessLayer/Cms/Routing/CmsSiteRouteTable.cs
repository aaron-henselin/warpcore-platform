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

        public bool TryGetRoute(Uri virtualPath, out SiteRoute route)
        {
            route = null;

            if (!_routesByPath.Contains(virtualPath))
                return false;

            route = _routesByPath[virtualPath].OrderBy(x => x.Priority).First();
            return true;
        }
    }
}