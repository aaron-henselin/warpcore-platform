using System;
using System.Linq;
using System.Web;
using WarpCore.Cms.Sites;

namespace DemoSite
{
    public static class SiteManagementContext
    {
        public static Site GetSiteToManage()
        {
            if (HttpContext.Current == null)
                return null;

            var siteRepository = new SiteRepository();
            var allSites = siteRepository.Find();

            Guid siteId = Guid.Empty;
            var siteToManageRaw = HttpContext.Current.Request["siteId"];
            if (!string.IsNullOrWhiteSpace(siteToManageRaw))
                siteId = new Guid(siteToManageRaw);
            else if (allSites.Any())
                siteId = allSites.First(x => x.IsFrontendSite).ContentId;

            return allSites.Single(x => x.ContentId == siteId);
        }
    }
}