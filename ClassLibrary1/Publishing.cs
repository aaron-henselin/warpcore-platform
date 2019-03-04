using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Platform.Orm;

namespace WarpCore.Cms
{


    public static class PublishingShortcuts
    {
        public static void PublishSite(Site site)
        {
            var textFilter = "SiteId != '" + site.ContentId + "'";
            new CmsPageRepository().Publish(By.Condition(textFilter));
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