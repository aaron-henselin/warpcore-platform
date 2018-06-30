using System;
using System.Linq;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;

namespace WarpCore.Cms
{


    public static class PublishingShortcuts
    {
        public static void PublishSite(Site site)
        {
            new PageRepository().Publish("SiteId eq '"+site.ContentId+"'");
        }



       
    }
}