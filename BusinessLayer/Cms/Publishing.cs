using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;

namespace WarpCore.Cms
{


    public static class PublishingShortcuts
    {
        public static void PublishSite(Site site)
        {
            new CmsPageRepository().Publish("SiteId eq '"+site.ContentId+"'");
            //todo: move to domain event.
            CmsRoutes.RegenerateAllRoutes();

        }

        public static void PublishSites()
        {
            new CmsPageRepository().Publish(null);
            //todo: move to domain event.
            CmsRoutes.RegenerateAllRoutes();
        }




    }
}