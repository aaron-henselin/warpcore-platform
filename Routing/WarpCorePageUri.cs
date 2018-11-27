using System;
using WarpCore.Platform.Kernel;

namespace WarpCore.Cms.Routing
{
    public class WarpCorePageUriTypeConverter : ITypeConverterExtension
    {
        public bool TryChangeType(object value, Type toType, out object newValue)
        {
            newValue = null;
            if (value is string && (toType == typeof(Uri) || toType == typeof(WarpCorePageUri)))
            {
                var uri = new Uri((string)value, UriKind.RelativeOrAbsolute);
                var isDataScheme = uri.IsWarpCoreDataScheme();
                if (isDataScheme)
                {
                    newValue= new WarpCorePageUri(uri.ToString());
                    return true;
                }

            }

            return false;
        }
    }

    public class WarpCorePageUri : Uri
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriString">A warpcore data uri in the format warpcore://repository/{apiId}/type/{apiId}/{contentId}</param>
        public WarpCorePageUri(string uriString) : base(uriString, UriKind.Absolute)
        {
        }

        public WarpCorePageUri(CmsPage draft) : this($"{UriSchemeWarpCorePage}://repository/{CmsPageRepository.ApiId}/type/{CmsPage.ApiId}/{draft.ContentId}")
        {
        }




        public const string UriSchemeWarpCorePage = "warpcore";


        public Guid RepositoryApiId => ExtractSegment(1);
        public Guid EntityApiId => ExtractSegment(3);
        public Guid ContentId => ExtractSegment(4);

        private Guid ExtractSegment(int i)
        {
            try
            {
                var segment = this.Segments[i].Trim('/');
                return new Guid(segment);
            }
            catch (Exception e)
            {
                throw new FormatException(base.ToString() + " is an invalid data uri. Expected format warpcore://repository/{apiId}/type/{apiId}/{contentId}.");
            }
        }

        public override string ToString()
        {
            if (RepositoryApiId == new Guid(CmsPageRepository.ApiId))
            {
                //I know this is weird, but it seems worth the tradeoff
                //to allow devs to use .ToString() directly in the markup.

                SiteRoute sr;
                var success = CmsRoutes.Current.TryResolveRoute(ContentId, out sr);
                if (success)
                {
                    var uriBuilderContext = Dependency.Resolve<UriBuilderContext>();
                    var builder = new CmsUriBuilder(uriBuilderContext);
                    var newUri = builder.CreateUriForRoute(sr, UriSettings.Default, null);
                    return newUri.ToString();
                }

            }

            //todo:
            //if it's content, find the best page to that that content is located on.
            return base.ToString();
        }

        public string ToDataUriString()
        {
            return base.ToString();
        }

    }
}