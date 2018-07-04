using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using Cms.Layout;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;

namespace DemoSite
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));
            
            SetupTestSite();

            PublishingShortcuts.PublishSites();
        }

        private Site SetupTestSite()
        {
            var tbx = new ToolboxManager();
            tbx.Save(new ToolboxItem
            {
                FullyQualifiedTypeName = typeof(Literal).AssemblyQualifiedName,
                Name="Literal"
            });

            var myLayout = new Layout
            {
                MasterPagePath = "/Demo.Master"
            };
            var layoutRepository = new LayoutRepository();
            layoutRepository.Save(myLayout);

            var siteRepo = new SiteRepository();
            var newSite = new Site
            {
                Name = "WarpCore Demo"
            };
            siteRepo.Save(newSite);

            var homePage = new CmsPage
            {
                Name = "Homepage",
                SiteId = newSite.ContentId.Value,
                LayoutId = myLayout.ContentId.Value
            };
            homePage.PageContent.Add(new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "Literal",
                Parameters = new Dictionary<string, string> {["Text"]="Hello World"}
            });

            var aboutUs = new CmsPage
            {
                Name = "About Us",
                SiteId = newSite.ContentId.Value,
                LayoutId = myLayout.ContentId.Value
            };
            var contactUs = new CmsPage
            {
                Name = "Contact Us",
                SiteId = newSite.ContentId.Value,
                LayoutId = myLayout.ContentId.Value
            };

            var pageRepository = new PageRepository();
            pageRepository.Save(homePage, SitemapRelativePosition.Root);
            pageRepository.Save(aboutUs, SitemapRelativePosition.Root);
            pageRepository.Save(contactUs, SitemapRelativePosition.Root);
            newSite.HomepageId = homePage.ContentId;
            siteRepo.Save(newSite);

            var subPage1 = new CmsPage
            {
                Name = "Subpage 1",
                SiteId = newSite.ContentId.Value
            };
            pageRepository.Save(subPage1, new PageRelativePosition { ParentPageId = homePage.ContentId });

            var subPage0 = new CmsPage
            {
                Name = "Subpage 0",
                SiteId = newSite.ContentId.Value
            };
            pageRepository.Save(subPage0, new PageRelativePosition { ParentPageId = homePage.ContentId, BeforePageId = subPage1.ContentId });

            return newSite;
        }

    }
}