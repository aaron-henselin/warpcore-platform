using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.DynamicData;
using System.Web.Routing;
using WarpCore.Cms.Routing;

namespace WarpCore.Cms
{
    public class RouteContext
    {
        public Guid? PageId { get; set; }
        public Guid? ContentId { get; set; }
    }

    public class RouteContextResolver
    {
        
        private void ResolveRoute(Uri uri)
        {
            //let's just take pages for now.
            var matchingSites = new SiteRepository()
                .GetAllSites()
                .Where(x => x.UriAuthority == uri.Authority || string.IsNullOrEmpty(x.UriAuthority))
                .OrderBy(x => x.Priority);

            var foundSite = matchingSites.First();
            var allRoutes = new RouteRepository().GetAllRoutes().OrderBy(x => x.Priority).FirstOrDefault(x => new Uri(x.SitemapPath, UriKind.Relative).AbsolutePath == uri.AbsolutePath);



        }
    }



    public class RouteTableBuilder
    {
        public void X()
        {
            RouteTable.Routes.RouteExistingFiles = true;

            var currentUrl = HttpContext.Current.Request.Url;
            RouteTable.Routes.MapPageRoute("DynamicRoute",
                "{DynamicPage}",
                "~/DynamicPage.aspx",false,null,new RouteValueDictionary{});
 
        }




    }
}
