using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Cms.Sites;

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

    public struct RouteConstraint
    {
        public string RoutePrefix { get; set; }
        public string UriAuthority { get; set; }
    }

    public class CmsRouteTable
    {

        private readonly Dictionary<RouteConstraint, CmsSiteRouteTable> _siteRouteTables = new Dictionary<RouteConstraint, CmsSiteRouteTable>();
       

        public void AddSubTable(Site site, CmsSiteRouteTable subRouteTable)
        {
            //var key = site.UriAuthority;
            //if (!string.IsNullOrWhiteSpace(site.RoutePrefix))
            //    key = key + site.RoutePrefix;

            _siteRouteTables.Add(new RouteConstraint{ RoutePrefix = site.RoutePrefix, UriAuthority = site.UriAuthority}, subRouteTable);

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

        private Uri RemoveTrailingSlashes(Uri inUri)
        {
            if (inUri.ToString().Length > 1)
                return new Uri(inUri.ToString().TrimEnd('/'), UriKind.Relative);

            return inUri;
        }

        public bool TryResolveRoute(Uri absoluteUri, out SiteRoute route)
        {
            if (!absoluteUri.IsAbsoluteUri)
                throw new Exception("Uris should be absolute.");


            route = null;

            RouteConstraint constraint;
            try
            {
                constraint = GetBestRouteConstraint(absoluteUri);
            }
            catch (Exception e)
            {
                return false;
            }

            var absPath = new Uri(absoluteUri.AbsolutePath, UriKind.Relative);
            if (constraint.RoutePrefix != null)
                absPath = new Uri(absPath.AbsolutePath.Remove(0, constraint.RoutePrefix.Length),UriKind.Relative);

            absPath = RemoveTrailingSlashes(absPath);

            return _siteRouteTables[constraint].TryGetRoute(absPath, out route);
        }

        private RouteConstraint GetBestRouteConstraint(Uri absoluteUri)
        {
            var allSites = _siteRouteTables.Keys;

            var matchingSites = allSites.Where(x => x.UriAuthority == absoluteUri.Authority 
                                                || x.UriAuthority == UriAuthorityFilter.Any)

                                        .Where(x => string.IsNullOrWhiteSpace(x.RoutePrefix)
                                                || absoluteUri.AbsolutePath.StartsWith(x.RoutePrefix,StringComparison.InvariantCultureIgnoreCase))

                                        .OrderByDescending(x => x.UriAuthority != UriAuthorityFilter.Any) //prefer most specific site
                                        .ThenByDescending(x => (x.RoutePrefix ?? "").Length) //prefer longest routeprefix
                                        .ToList();

            if (!matchingSites.Any())
                throw new Exception("Could not match absolute uri to a constraint.");

            return matchingSites.First();
        }

    }
}