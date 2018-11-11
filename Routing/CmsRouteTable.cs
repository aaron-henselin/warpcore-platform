using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Cms.Sites;

namespace WarpCore.Cms.Routing
{
    public static class CmsRoutes
    {
        private static CmsRouteTable _current;

        public static void RegenerateAllRoutes()
        {
            _current = null;
        }

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


        private string NormalizeRoutePrefix(string routePrefix)
        {
            if (string.IsNullOrWhiteSpace(routePrefix))
                return null;

            return routePrefix.Trim('/');
        }


        public void AddSubTable(Site site, CmsSiteRouteTable subRouteTable)
        {
            //var key = site.UriAuthority;
            //if (!string.IsNullOrWhiteSpace(site.RoutePrefix))
            //    key = key + site.RoutePrefix;

            _siteRouteTables.Add(new RouteConstraint{ RoutePrefix = NormalizeRoutePrefix(site.RoutePrefix), UriAuthority = site.UriAuthority}, subRouteTable);

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
            //if (constraint.RoutePrefix != null)
            //{
            //    var routePrefixRemoved = absoluteUri.AbsolutePath.Remove(0, constraint.RoutePrefix.Length+1);
            //    absPath = new Uri(routePrefixRemoved, UriKind.Relative);
            //}

            absPath = RemoveTrailingSlashes(absPath);

            return _siteRouteTables[constraint].TryGetRoute(absPath, out route);
        }

        private IEnumerable<RouteConstraint> FilterRouteContraintsByRoutePrefix(Uri absoluteUri, IEnumerable<RouteConstraint> allContraints)
        {
            foreach (var route in allContraints)
            {
                if (route.UriAuthority != UriAuthorityFilter.Any && route.UriAuthority != absoluteUri.Authority)
                    continue;

                if (string.IsNullOrWhiteSpace(route.RoutePrefix))
                {
                    yield return route;
                    continue;
                }

                if (absoluteUri.AbsolutePath.Equals("/" + route.RoutePrefix,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return route;
                    continue;
                }

                if (absoluteUri.AbsolutePath.StartsWith("/" + route.RoutePrefix + "/",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return route;
                    continue;
                }

            }
        }

        private RouteConstraint GetBestRouteConstraint(Uri absoluteUri)
        {
            var allSites = _siteRouteTables.Keys;

            var matchingSites = FilterRouteContraintsByRoutePrefix(absoluteUri,allSites)
                                        .OrderByDescending(x => x.UriAuthority != UriAuthorityFilter.Any) //prefer most specific site
                                        .ThenByDescending(x => (x.RoutePrefix ?? "").Length) //prefer longest routeprefix
                                        .ToList();

            if (!matchingSites.Any())
                throw new Exception("Could not match absolute uri to a constraint.");

            return matchingSites.First();
        }

    }
}