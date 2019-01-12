namespace WarpCore.Cms
{
    //public class RouteContextResolver
    //{

    //    private IEnumerable<SiteRoute> BuildRoutes()
    //    {
    //        List<SiteRoute> siteRoutes = new List<SiteRoute>();

    //        var sites = new SiteRepository()
    //            .GetAllSites();

    //        //.Where(x => x.UriAuthority == uri.Authority || string.IsNullOrEmpty(x.UriAuthority))
    //        //.OrderBy(x => x.Priority);

    //        var pageRepo = new CmsPageRepository();
    //        var foundPages = pageRepo.GetAllPages().ToLookup(x => x.SiteId);

    //        var pageRoutes = new RouteRepository().GetAllRoutes().ToLookup(x => x.PageId);

    //        foreach (var site in sites)
    //        {
    //            var sitePages = foundPages[site.Id];
    //            foreach (var page in sitePages)
    //            {
    //                var relatedRoutes = pageRoutes[page.Id];
    //                foreach (var route in relatedRoutes)
    //                {
    //                    var pageRoute = new SiteRoute
    //                    {
    //                        Authority = site.UriAuthority,
    //                        Priority = route.Priority,
    //                        SiteId = site.Id,
    //                        ContentId = null,
    //                        VirtualPath = MakeAbsoluteUri(site, route.Slug)
    //                    };

    //                    siteRoutes.Add(pageRoute);
    //                }

    //                //page.PageContent.First().WidgetTypeCode
    //            }
    //        }



    //    }

    //    private Uri MakeAbsoluteUri(Site site, string path)
    //    {
    //        var rawUri = "/" + site.RoutePrefix + "/" + path;
    //        return new Uri(rawUri, UriKind.Relative);
    //    }

    //}
}