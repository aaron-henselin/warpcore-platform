using System;
using System.Linq;
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

            var subPage1 = new CmsPage
            {
                Name = "Subpage 1",
                SiteId = newSite.ContentId.Value
            };
            pageRepository.Save(subPage1, new PageRelativePosition{ParentPageId = homePage.ContentId});

            var subPage0 = new CmsPage
            {
                Name = "Subpage 0",
                SiteId = newSite.ContentId.Value
            };
            pageRepository.Save(subPage0, new PageRelativePosition { ParentPageId = homePage.ContentId,BeforePageId = subPage1.ContentId});


            var liveSitemap_before = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live);
            Assert.AreEqual(0, liveSitemap_before.ChildNodes.Count);

            PublishingShortcuts.PublishSite(newSite);


            var structure = SiteStructureMapBuilder.BuildStructureMap(newSite);
            Assert.AreEqual(3,structure.ChildNodes.Count);

            Assert.AreEqual(subPage0.ContentId, structure.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).PageId);
            Assert.AreEqual(subPage1.ContentId, structure.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).PageId);


            var liveSitemap = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live);
            Assert.AreEqual(3, liveSitemap.ChildNodes.Count);


            pageRepository.Move(subPage0,SitemapRelativePosition.Root);

            SiteRoute sr;
            var success = CmsRoutes.Current.TryResolveRoute(new Uri("/",UriKind.Relative),out sr);
            Assert.IsTrue(success);

            //var allRoutes = RouteBuilder.DiscoverRoutesForSite(newSite);
            //new CmsRouteTable();

        }
    }
}
