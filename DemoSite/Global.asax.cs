using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using Cms.DynamicContent;
using Cms.Forms;
using Cms.Layout;
using Cms.Toolbox;
using Framework;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Widgets;
using WarpCore.Web.Widgets.FormBuilder;

namespace DemoSite
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));
            
            BootEvents.RegisterSiteBootAction(() =>
            {
                SetupDynamicTypes();
                SetupCustomFields();
                SetupToolbox();
                SetupBackendSite();
                

                PublishingShortcuts.PublishSites();
                SetupTestSite();
            });


        }

        private void SetupDynamicTypes()
        {
            var fullDynamicTypeId = Guid.NewGuid();

            var repositories = new RepositoryMetadataManager();
            repositories.Save(new RepositoryMetdata
            {
                CustomRepositoryName = "Article",
                IsDynamic = true,
                ApiId = fullDynamicTypeId.ToString()
            });

            var mgr = new ContentInterfaceRepository();
            var ext1 = new ContentInterface
            {
                ContentTypeId = fullDynamicTypeId,
                InterfaceName = KnownTypeExtensionNames.CustomFields
            };
            var ext2 = new ContentInterface
            {
                ContentTypeId = fullDynamicTypeId,
                InterfaceName = "SomePluginInfo"
            };
            mgr.Save(ext1);
            mgr.Save(ext2);

            var extension = mgr.GetCustomFieldsTypeExtension(fullDynamicTypeId);
            extension.InterfaceFields.Add(new ChoiceInterfaceField
            {
                PropertyName = "IsFeatured",
                PropertyTypeName = typeof(bool).FullName
            });

            mgr.Save(extension);

            var repoType = RepositoryTypeResolver.ResolveDynamicTypeByInteropId(fullDynamicTypeId);
            var repo = (IVersionedContentRepositoryBase)Activator.CreateInstance(repoType);

            //var repo = DynamicContentManager.ActivateDynamicRepository(newType.TypeResolverUid);
            repo.Save(new DynamicVersionedContent(fullDynamicTypeId));
            var drafts = repo.FindContentVersions("", ContentEnvironment.Draft);
            if (!drafts.Any())
                throw new Exception();
        }

        private void SetupCustomFields()
        {

            var mgr = new ContentInterfaceRepository();
            var extension = mgr.GetCustomFieldsTypeExtension(new Guid(CmsPage.ApiId));
            extension.InterfaceFields.Add(new ChoiceInterfaceField
            {
                PropertyName = "DisplayInNav",
                PropertyTypeName = typeof(bool).FullName
            });

            mgr.Save(extension);
        }

        private void SetupToolbox()
        {
            var tbx = new ToolboxManager();
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/PageTree.ascx",
                WidgetUid = "wc-pagetree",
                FriendlyName = "Page Tree"
            });
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/EntityBuilder.ascx",
                WidgetUid = "wc-entitybuilder",
                FriendlyName = "Entity Builder"
            });
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/FormDesigner.ascx",
                WidgetUid = "wc-formdesigner",
                FriendlyName = "Form Designer"
            });
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/Forms/DynamicForm.ascx",
                WidgetUid = "wc-dynamic-form",
                FriendlyName = "Dynamic Form"
            });
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/EntityList.ascx",
                WidgetUid = "wc-entity-list",
                FriendlyName = "Entity List"
            });
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/ContentList.ascx",
                WidgetUid = "wc-content-list",
                FriendlyName = "Content List"
            });



        }

        private CmsForm SetupPageSettingsForm()
        {
            var form = new CmsForm
            {
                ContentId = Guid.NewGuid(),
                Name = "Page Settings",
                RepositoryUid = new Guid("979fde2a-1983-480e-aca4-8caab3f762b0"),
            };

            CmsPageContentFactory factory = new CmsPageContentFactory();

            var twoColumn =
            factory.CreateToolboxItemContent(new RowLayout { NumColumns = 2 });
            twoColumn.PlacementContentPlaceHolderId = ConfiguratorFormBuilder.RuntimePlaceHolderId;
            form.FormContent.Add(twoColumn);
            
            var textboxPageContent =
                factory.CreateToolboxItemContent(new ConfiguratorTextBox
                {
                    PropertyName = nameof(CmsPage.Name),
                    DisplayName = "Page Name",
                    
                });

            textboxPageContent.PlacementContentPlaceHolderId = "0";
            twoColumn.AllContent.Add(textboxPageContent);


            var layoutDropDown =
            factory.CreateToolboxItemContent(new ConfiguratorDropDownList
            {
                PropertyName = nameof(CmsPage.LayoutId),
                DisplayName = "Layout",
            });
            layoutDropDown.PlacementContentPlaceHolderId = "1";
            twoColumn.AllContent.Add(layoutDropDown);

            var oneColumn =
factory.CreateToolboxItemContent(new RowLayout { NumColumns = 1 });
            oneColumn.PlacementContentPlaceHolderId = ConfiguratorFormBuilder.RuntimePlaceHolderId;
            form.FormContent.Add(oneColumn);

            var keywords =
                factory.CreateToolboxItemContent(new ConfiguratorTextBox
                {
                    PropertyName = nameof(CmsPage.Keywords),
                    DisplayName = "Keywords",
                });
            keywords.PlacementContentPlaceHolderId = "0";
            oneColumn.AllContent.Add(keywords);

            var description =
                factory.CreateToolboxItemContent(new ConfiguratorTextBox
                {
                    PropertyName = nameof(CmsPage.Description),
                    DisplayName = "Description",
                });
            oneColumn.AllContent.Add(description);



            var formRepository = new FormRepository();
            formRepository.Save(form);
            formRepository.Publish(By.ContentId(form.ContentId));

            return form;
        }

        private void SetupBackendSite()
        {
  

            var backendLayout = new Layout
            {
                Name = "Backend Layout",
                MasterPagePath = "/App_Data/BackendPage.Master"
            };


            var layoutRepository = new LayoutRepository();
            layoutRepository.Save(backendLayout);
            

            var siteRepo = new SiteRepository();
            var backendSite = new Site
            {
                Name = "Admin",
                RoutePrefix = "Admin",
                ContentId = new Guid("00000000-0000-0000-0000-000000000001"),
            };
            siteRepo.Save(backendSite);


            var formDesigner = new CmsPage
            {
                Name = "Form Designer",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false
            };
            formDesigner.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-formdesigner"
            });


            var pageTree = new CmsPage
            {
                Name = "Pages",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId
            };
            pageTree.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-pagetree"
            });


            var pageSettingsForm = SetupPageSettingsForm();


            var pageSettings = new CmsPage
            {
                Name = "Settings",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false
            };
            pageSettings.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-dynamic-form",
                Parameters = new Dictionary<string, string>{["FormId"]= pageSettingsForm.ContentId.ToString()}
            });


            var entityBuilderPage = new CmsPage
            {
                Name = "Entity Builder",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId
            };
            entityBuilderPage.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-entitybuilder",
                Parameters = new Dictionary<string, string> { }
            });

            var entityListPage = new CmsPage
            {
                Name = "Entities",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId
            };
            entityListPage.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-entity-list",
                Parameters = new Dictionary<string, string> { }
            });

            

            ////////////////
            var dynamicListTest = new CmsPage
            {
                Name = "Dynamic List Test",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = true
            };

            var contentListControl= new ContentList()
            {
                RepositoryId = new Guid(CmsPageRepository.ApiId),
                Config = new ContentListConfiguration
                {
                    Fields = new List<ContentListField>
                    {
                        new ContentListField
                        {
                            Header = "Name",
                            Template = "{" + nameof(CmsPage.Name) + "}"
                        }

                    }
                }
            };
            var parameters= (Dictionary<string,string>)contentListControl.GetPropertyValues(ToolboxPropertyFilter.IsConfigurable);


            dynamicListTest.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-content-list",
                Parameters = parameters
            });


            ////////////////
            var formsList = new CmsPage
            {
                Name = "Forms",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = true
            };

                
                
            var formList = new ContentList()
            {
                RepositoryId = new Guid(FormRepository.ApiId),
                Config = new ContentListConfiguration
                {
                    Fields = new List<ContentListField>
                    {
                        new ContentListField
                        {
                            Header = "Name",
                            Template =nameof(CmsForm.Name)
                        },
                        new ContentListField
                        {
                            Header = "Actions",
                            Template =@"<a href='/Admin/form-designer?formId={"+nameof(CmsForm.DesignForContentId)+"}'><span class='glyphicon glyphicon-text-background'></span>Design</a>"
                        }

                    }
                }
            };
            var formListParameters = (Dictionary<string, string>)formList.GetPropertyValues(ToolboxPropertyFilter.IsConfigurable);

            formsList.PageContent.Add(new CmsPageContent
            {
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-content-list",
                Parameters = formListParameters
            });



            var pageRepo = new CmsPageRepository();
            pageRepo.Save(pageTree);
            pageRepo.Save(formDesigner);
            pageRepo.Save(pageSettings);
            pageRepo.Save(entityBuilderPage);
            pageRepo.Save(entityListPage);
            pageRepo.Save(dynamicListTest);
            pageRepo.Save(formsList);
            backendSite.HomepageId = pageTree.ContentId;
            siteRepo.Save(backendSite);

            var editBackendPageTreeLink = new CmsPage
            {
                Name = "Edit Backend Pages",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                PageType = PageType.RedirectPage,
                RedirectPageId = pageTree.ContentId,
                InternalRedirectParameters = new Dictionary<string, string> { ["SiteId"]= backendSite.ContentId.ToString()}
            };

            pageRepo.Save(editBackendPageTreeLink);

        }

        private Site SetupTestSite()
        {
            var tbx = new ToolboxManager();

            var myLayout = new Layout
            {
                Name = "Demo",
                MasterPagePath = "/Demo.Master",

            };

            myLayout.PageContent.Add(new CmsPageContent{WidgetTypeCode = "Client-CustomNavigation",PlacementContentPlaceHolderId = "NavigationContentPlaceHolder"});
            var layoutRepository = new LayoutRepository();
            layoutRepository.Save(myLayout);

            var siteRepo = new SiteRepository();
            var newSite = new Site
            {
                Name = "WarpCore Demo",
                IsFrontendSite = true
            };
            siteRepo.Save(newSite);

            var homePage = new CmsPage
            {
                Name = "Homepage",
                SiteId = newSite.ContentId,
                LayoutId = myLayout.ContentId,
                Keywords = "WarpCore,CMS,Demo",
               Description = "WarpCore CMS Demo"

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
                SiteId = newSite.ContentId,
                LayoutId = myLayout.ContentId
            };
            var contactUs = new CmsPage
            {
                Name = "Contact Us",
                SiteId = newSite.ContentId,
                LayoutId = myLayout.ContentId
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

    }
}