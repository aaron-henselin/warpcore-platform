using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Http;
using System.Web.Script.Serialization;
using WarpCore.Cms.Sites;

namespace WarpCore.Cms.Routing
{
    public class DefaultValueCollection : Dictionary<string, string>
    {
        public override string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }

        public static DefaultValueCollection FromString(string value)
        {
            return new JavaScriptSerializer().Deserialize<DefaultValueCollection>(value);
        }

    }


    public class DynamicFormRequestContext
    {
        public DefaultValueCollection DefaultValues { get; set; }
        public Guid? ContentId { get; set; }
    }

    public class UriBuilderContext
    {
        public bool IsSsl { get; set; }
        public string Authority { get; set; }
        public Uri AbsolutePath { get; set; }
    }

    public enum UriKinds
    {
        AbsoluteUri, RelativeFromSiteRoot,RelativeFromCurrent
    }

    public class UriSettings
    {
        public UriKinds PreferUriKind { get; set; }

        public static UriSettings Default => new UriSettings();
    }

    public class CmsUriBuilder
    {
        private readonly UriBuilderContext _context;

        public CmsUriBuilder(UriBuilderContext context)
        {
            _context = context;
        }

        public Uri CreateUriForRoute(SiteRoute sr, UriSettings settings, IDictionary<string,string> parameters)
        {
            var requireSsl = false;
            if (sr is ContentPageRoute cpr)
                requireSsl = cpr.RequireSsl;

            var needsAuthorityChange = (sr.Authority != UriAuthorityFilter.Any && _context.Authority != sr.Authority);
            var needsSslChange = !_context.IsSsl && requireSsl;

            if (settings.PreferUriKind != UriKinds.AbsoluteUri && !needsSslChange && !needsAuthorityChange)
            {
                if (settings.PreferUriKind == UriKinds.RelativeFromSiteRoot)
                    return sr.VirtualPath;

                if (settings.PreferUriKind == UriKinds.RelativeFromCurrent)
                    return _context.AbsolutePath.MakeRelativeUri(sr.VirtualPath);
            }


            var protocol = "http://";
            if (requireSsl)
                protocol = "https://";

            var authority = sr.Authority;
            if (sr.Authority == UriAuthorityFilter.Any)
                authority = _context.Authority;

            var uriBase = new Uri($"{protocol}{authority}{sr.VirtualPath}");
            var builder = new UriBuilder(uriBase);
            if (parameters != null && parameters.Count > 0)
                builder.Query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            return builder.Uri;
        }


        public Uri CreateUri(CmsPage destinationPage, UriSettings settings, IDictionary<string,string> parameters)
        {
            SiteRoute sr;
            var success = CmsRoutes.Current.TryResolveRoute(destinationPage.ContentId, out sr);
            if (!success)
                throw new Exception($"Could not resolve a content route for {destinationPage.Name}, id: {destinationPage.ContentId}");

            return CreateUriForRoute(sr, settings, parameters);
        }


    }
}