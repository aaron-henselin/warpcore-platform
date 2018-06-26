using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarpCore.Cms;
using WarpCore.Crm;
using WarpCore.DbEngines.AzureStorage;

namespace IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));

            var siteRepo = new SiteRepository();
            var newSite = new Site
            {
                Name = "WarpCore Demo"
            };
            siteRepo.Save(newSite);

            var homePage = new CmsPage
            {
                Name = "Homepage",
                SiteId = newSite.ContentId.Value
            };
            var aboutUs = new CmsPage
            {
                Name = "About Us",
                SiteId = newSite.ContentId.Value
            };
            var contactUs = new CmsPage
            {
                Name = "Contact Us",
                SiteId = newSite.ContentId.Value
            };

            var pageRepository = new PageRepository();
            pageRepository.Save(homePage,SitemapRelativePosition.Root);
            pageRepository.Save(aboutUs, SitemapRelativePosition.Root);
            pageRepository.Save(contactUs, SitemapRelativePosition.Root);
            newSite.HomepageId = homePage.ContentId;
            siteRepo.Save(newSite);

            var liveSitemap_before = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live);
            Assert.AreEqual(0, liveSitemap_before.ChildNodes.Count);

            PublishingShortcuts.PublishSite(newSite);

            var structure = new SiteStructureMapBuilder().BuildStructureMap(newSite);
            Assert.AreEqual(3,structure.ChildNodes.Count);

            var liveSitemap = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live);
            Assert.AreEqual(3, liveSitemap.ChildNodes.Count);


            


        }
    }
}
