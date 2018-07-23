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
using WarpCore.Web.Widgets;

namespace DemoSite
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));
            
            SetupToolbox();
          
            SetupBackendSite();

            PublishingShortcuts.PublishSites();
            SetupTestSite();
        }

        private void SetupToolbox()
        {
            var tbx = new ToolboxManager();
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/PageTree.ascx",
                WidgetUid = "PageTree",
                FriendlyName = "Page Tree"
            });
        }

        private void SetupBackendSite()
        {
  

            var backendLayout = new Layout
            {
                MasterPagePath = "/App_Data/BackendPage.Master"
            };


            var layoutRepository = new LayoutRepository();
            layoutRepository.Save(backendLayout);

     
            var siteRepo = new SiteRepository();
            var backendSite = new Site
            {
                Name = "Admin",
                RoutePrefix = "Admin"
            };
            siteRepo.Save(backendSite);


            var pageTree = new CmsPage
            {
                Name = "PageTree",
                SiteId = backendSite.ContentId.Value,
                LayoutId = backendLayout.ContentId.Value
            };
            pageTree.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "PageTree"

            });

            var pageRepo = new PageRepository();
            pageRepo.Save(pageTree);

            backendSite.HomepageId = pageTree.ContentId;
            siteRepo.Save(backendSite);
        }

        private Site SetupTestSite()
        {
            var tbx = new ToolboxManager();

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

            var lbId = Guid.NewGuid();
            var row = new CmsPageContent
            {
                Id = Guid.NewGuid(),
                WidgetTypeCode = "WC/RowLayout",
                PlacementContentPlaceHolderId = "Body",
                Parameters = new Dictionary<string, string>
                {
                    //[nameof(RowLayout.LayoutBuilderId)] = lbId.ToString(),
                    [nameof(RowLayout.NumColumns)] = 3.ToString()
                }
            };

            var helloWorld0 = new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "0",
                PlacementLayoutBuilderId = lbId,
                WidgetTypeCode = "WC/ContentBlock",
                Parameters = new Dictionary<string, string> {["AdHocHtml"] = "Hello World (0)"}
            };

            var helloWorld1 = new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "1",
                PlacementLayoutBuilderId = lbId,
                WidgetTypeCode = "WC/ContentBlock",
                Parameters = new Dictionary<string, string> { ["AdHocHtml"] = "Hello World (1)" }
            };

            row.AllContent.Add(helloWorld0);
            row.AllContent.Add(helloWorld1);

            homePage.PageContent.Add(row);

            




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