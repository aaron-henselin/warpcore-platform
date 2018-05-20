using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.Cms
{
    public class CmsSiteRouteTable
    {
        private ILookup<Uri, SiteRoute> _toLookup;

        public CmsSiteRouteTable(List<SiteRoute> siteRoutes)
        {
            _toLookup = siteRoutes.ToLookup(x => x.VirtualPath);
        }

        public bool TryGetRoute(Uri virtualPath, out SiteRoute route)
        {
            route = null;

            if (!_toLookup.Contains(virtualPath))
                return false;

            route = _toLookup[virtualPath].OrderBy(x => x.Priority).First();
            return true;
        }
    }
}