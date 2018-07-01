using System;
using System.Linq;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{


    public static class PublishingShortcuts
    {
        public static void PublishSite(Site site)
        {
            new PageRepository().Publish("SiteId eq '"+site.ContentId+"'");
            //todo: move to domain event.
            CmsRoutes.RegenerateAllRoutes();
        }

        public static void PublishSites()
        {
            new PageRepository().Publish(null);
            //todo: move to domain event.
            CmsRoutes.RegenerateAllRoutes();
        }




    }
}