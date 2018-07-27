using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;

namespace IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        private Site SetupTestSite()
        {
            var siteRepo = new SiteRepository();
            var newSite = new Site
            {
                Name = "WarpCore Demo"
            };
            siteRepo.Save(newSite);

            var homePage = new CmsPage
            {
                Name = "Homepage",
                SiteId = newSite.ContentId
            };
            var aboutUs = new CmsPage
            {
                Name = "About Us",
                SiteId = newSite.ContentId
            };
            var contactUs = new CmsPage
            {
                Name = "Contact Us",
                SiteId = newSite.ContentId
            };

            var pageRepository = new CmsPageRepository();
            pageRepository.Save(homePage, SitemapRelativePosition.Root);
            pageRepository.Save(aboutUs, SitemapRelativePosition.Root);
            pageRepository.Save(contactUs, SitemapRelativePosition.Root);
            newSite.HomepageId = homePage.ContentId;
            siteRepo.Save(newSite);

            var subPage1 = new CmsPage
            {
                Name = "Subpage 1",
                SiteId = newSite.ContentId
            };
            pageRepository.Save(subPage1, new PageRelativePosition { ParentPageId = homePage.ContentId });

            var subPage0 = new CmsPage
            {
                Name = "Subpage 0",
                SiteId = newSite.ContentId
            };
            pageRepository.Save(subPage0, new PageRelativePosition { ParentPageId = homePage.ContentId, BeforePageId = subPage1.ContentId });

            return newSite;
        }

        private static void AssertUrlIsRoutable(params string[] uris)
        {
            foreach (var uri in uris)
            {
                var success = CmsRoutes.Current.TryResolveRoute(new Uri(uri, UriKind.Absolute), out _);
                Assert.IsTrue(success);
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));

            var newSite = SetupTestSite();


            var liveSitemapBefore = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live);
            Assert.AreEqual(0, liveSitemapBefore.ChildNodes.Count);

            PublishingShortcuts.PublishSite(newSite);


            var structure = SiteStructureMapBuilder.BuildStructureMap(newSite);
            Assert.AreEqual(3, structure.ChildNodes.Count);


            //Assert.AreEqual(subPage0.ContentId, structure.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).PageId);
            //Assert.AreEqual(subPage1.ContentId, structure.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).PageId);


            var liveSitemap = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live);
            Assert.AreEqual(3, liveSitemap.ChildNodes.Count);


            //pageRepository.Move(subPage0,SitemapRelativePosition.Root);

            AssertUrlIsRoutable(
                "http://www.google.com",
                "http://www.google.com/",
                "http://www.google.com/homepage/subpage-0",
                "http://www.google.com/homepage/subpage-0/");

            var lastPage = liveSitemap.ChildNodes.Last().Page;


            var uriBuilder = new CmsUriBuilder(
                    new UriBuilderContext
                    {
                        AbsolutePath = new Uri("/",UriKind.Relative),
                        Authority = "www.google.com",
                        IsSsl = false
                    }
                );

            uriBuilder.CreateUri(lastPage,UriSettings.Default);

        }
    }
}
