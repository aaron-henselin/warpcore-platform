using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WarpCore.Cms.Sites;

namespace WarpCore.Cms.Routing
{
    public class UriBuilderContext
    {
        public bool IsSsl { get; set; }
        public string Authority { get; set; }
        public string AbsolutePath { get; set; }
    }

    public class UriSettings
    {
        public bool PreferAbsoluteUri { get; set; }

        public static UriSettings Default => new UriSettings();
    }

    public class CmsUriBuilder
    {
        private readonly UriBuilderContext _context;

        public CmsUriBuilder(UriBuilderContext context)
        {
            _context = context;
        }

        public Uri CreateUriForRoute(SiteRoute sr, UriSettings settings)
        {
            var requireSsl = false;
            if (sr is ContentPageRoute cpr)
                requireSsl = cpr.RequireSsl;

            var needsAuthorityChange = (sr.Authority != UriAuthorityFilter.Any && _context.AbsolutePath != sr.Authority);
            var needsSslChange = !_context.IsSsl && requireSsl;

            if (!settings.PreferAbsoluteUri && !needsSslChange && !needsAuthorityChange)
                return sr.VirtualPath;

            var protocol = "http://";
            if (requireSsl)
                protocol = "https://";

            var authority = sr.Authority;
            if (sr.Authority == UriAuthorityFilter.Any)
                authority = _context.Authority;

            return new Uri(protocol + authority + sr.VirtualPath, UriKind.Absolute);
        }

        public Uri CreateUri(CmsPage destinationPage, UriSettings settings)
        {
            SiteRoute sr;
            var success = CmsRoutes.Current.TryResolveRoute(destinationPage.ContentId.Value, out sr);
            if (!success)
                throw new Exception();

            var cpr = (ContentPageRoute) sr;
            return CreateUriForRoute(cpr, settings);
        }
    }
}