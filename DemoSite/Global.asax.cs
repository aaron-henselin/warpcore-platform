﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using BlazorComponents.Shared;
using Cms;
using Cms.Forms;
using Cms.Layout;
using Cms_StaticContent_RenderingEngine;
using DemoSite;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Configuration;
using Modules.Cms.Features.Presentation.Page.Elements;
using Modules.Cms.Features.Presentation.PageComposition;
using Modules.Cms.Features.Presentation.RenderingEngines.Mvc;
using Modules.Cms.Features.Presentation.RenderingEngines.Mvc.Toolset.Controllers;
using Modules.Cms.Features.Presentation.RenderingEngines.WebForms;
using Platform_Hosting_AspNet;
using Platform_Hosting_AspNet.AspNet;
using Platform_WebPipeline;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.EmbeddedResourceVirtualPathProvider;
using WarpCore.Web.Widgets.FormBuilder.Support;

[assembly: PreApplicationStartMethod(typeof(ActualBootstrapper), nameof(ActualBootstrapper.PreInitialize))]

namespace DemoSite
{
    public static class ActualBootstrapper
    {
        public static void PreInitialize()
        {
            
            var webStack = new AspNetWebStack();
            WebBootstrapper.PreInitializeWebStack(webStack);
            
            CmsConfiguration.Current.AddMvcSupport();
            CmsConfiguration.Current.AddWebFormsSupport();
            CmsConfiguration.Current.AddStaticContentSupport();

            ToolboxPropertyFilter.BlacklistedDeclaringAssemblies.AddRange(new[]
            {
                typeof(Control).Assembly,
                typeof(Controller).Assembly,
                typeof(WarpCoreEntity).Assembly
            });







            Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));

            DomainEvents.Subscribe<SiteBootCompleted>(_ =>
            {
                var demoInstaller = new DemoInstaller();
                demoInstaller.SetupDynamicTypes();
                demoInstaller.SetupCustomFields();
                demoInstaller.SetupToolbox();
                demoInstaller.SetupBackendSite();


                PublishingShortcuts.PublishSites();
                demoInstaller.SetupTestSite();
            });

        }
    }

    public class DemoInstaller
    {
        public void SetupDynamicTypes()
        {
            var fullDynamicTypeId = Guid.NewGuid();

            var repositories = new RepositoryMetadataManager();
            repositories.Save(new RepositoryMetdata
            {
                CustomRepositoryName = "Article",
                IsDynamic = true,
                ApiId = fullDynamicTypeId
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

            var repo = RepositoryActivator.ActivateRepository<ISupportsCmsForms>(fullDynamicTypeId);

            repo.Save(new DynamicVersionedContent(fullDynamicTypeId));
            //var drafts = repo.FindContentVersions("", ContentEnvironment.Draft);
            //if (!drafts.Any())
            //    throw new Exception();
        }

        public void SetupCustomFields()
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

        public void SetupToolbox()
        {
            var tbx = new ToolboxManager();
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
            //tbx.Save(new ToolboxItem
            //{
            //    AscxPath = "/App_Data/Forms/DynamicForm.ascx",
            //    WidgetUid = DynamicForm.ApiId,
            //    FriendlyName = "Dynamic Form"
            //});
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/EntityList.ascx",
                WidgetUid = "wc-entity-list",
                FriendlyName = "Entity List"
            });


            //
            tbx.Save(new ToolboxItem
            {
                AscxPath = "/App_Data/BackendWidgets/AddPageWizard.ascx",
                WidgetUid = AddPageWizard.ApiId,
                FriendlyName = "Add Page Wizard"
            });

        }

        private CmsForm SetupRedirectPageSettingsForm()
        {
            var form = new CmsForm
            {
                ContentId = KnownFormIds.RedirectPageSettingsForm,
                Name = "Redirect Page Settings",
                RepositoryUid = new Guid(CmsPageRepository.ApiId),
            };

            //CmsPageContentBuilder builder = new CmsPageContentBuilder();

            //var twoColumn =
            //    builder.BuildCmsPageContentFromWebFormsControl(new RowLayout { NumColumns = 2 });
            //twoColumn.PlacementContentPlaceHolderId = ConfiguratorFormBuilder.RuntimePlaceHolderId;
            //form.FormContent.Add(twoColumn);


            //var textboxPageContent =
            //    builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorTextBox
            //    {
            //        PropertyName = nameof(CmsPage.Name),
            //        DisplayName = "Page Name",

            //    });

            //textboxPageContent.PlacementContentPlaceHolderId = "0";

            //var urlSelector =
            //    builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorUrlSelector()
            //    {
            //        PropertyName = nameof(CmsPage.RedirectUri),
            //        DisplayName = "Redirect To",

            //    });

            //textboxPageContent.PlacementContentPlaceHolderId = "0";

            //twoColumn.AllContent.Add(textboxPageContent);
            //twoColumn.AllContent.Add(urlSelector);


            //var siteIdDropdown =
            //    builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorHiddenField()
            //    {
            //        PropertyName = nameof(CmsPage.SiteId)
            //    });
            //twoColumn.AllContent.Add(siteIdDropdown);

            var formRepository = new FormRepository();
            formRepository.Save(form);
            formRepository.Publish(By.ContentId(form.ContentId));

            return form;
        }

        private CmsForm SetupGroupingPageSettingsForm()
        {
            var form = new CmsForm
            {
                ContentId = KnownFormIds.GroupingPageSettingsForm,
                Name = "Grouping Page Settings",
                RepositoryUid = new Guid(CmsPageRepository.ApiId),
            };

            //CmsPageContentBuilder builder = new CmsPageContentBuilder();

            //var twoColumn =
            //    builder.BuildCmsPageContentFromWebFormsControl(new RowLayout { NumColumns = 2 });
            //twoColumn.PlacementContentPlaceHolderId = ConfiguratorFormBuilder.RuntimePlaceHolderId;
            //form.FormContent.Add(twoColumn);


            //var textboxPageContent =
            //    builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorTextBox
            //    {
            //        PropertyName = nameof(CmsPage.Name),
            //        DisplayName = "Page Name",

            //    });

            //textboxPageContent.PlacementContentPlaceHolderId = "0";
            //twoColumn.AllContent.Add(textboxPageContent);


            //var siteIdDropdown =
            //    builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorHiddenField()
            //    {
            //        PropertyName = nameof(CmsPage.SiteId)
            //    });
            //twoColumn.AllContent.Add(siteIdDropdown);

            var formRepository = new FormRepository();
            formRepository.Save(form);
            formRepository.Publish(By.ContentId(form.ContentId));

            return form;
        }

        private CmsForm SetupPageSettingsForm()
        {
            var form = new ConfiguratorCmsPageContentBuilder().GenerateDefaultForm(typeof(CmsPage),FormStyle.Edit);


            form.ContentId = KnownFormIds.ContentPageSettingsForm;
            form.Name = "Content Page Settings";
            form.RepositoryUid = new Guid(CmsPageRepository.ApiId);






            //            CmsPageContentBuilder builder = new CmsPageContentBuilder();

            //            var twoColumn =
            //            builder.BuildCmsPageContentFromWebFormsControl(new RowLayout { NumColumns = 2 });
            //            twoColumn.PlacementContentPlaceHolderId = ConfiguratorFormBuilder.RuntimePlaceHolderId;
            //            form.FormContent.Add(twoColumn);


            //            var textboxPageContent =
            //                builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorTextBox
            //                {
            //                    PropertyName = nameof(CmsPage.Name),
            //                    DisplayName = "Page Name",

            //                });

            //            textboxPageContent.PlacementContentPlaceHolderId = "0";
            //            twoColumn.AllContent.Add(textboxPageContent);


            //            var layoutDropDown =
            //            builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorDropDownList
            //            {
            //                PropertyName = nameof(CmsPage.LayoutId),
            //                DisplayName = "Layout",
            //            });
            //            layoutDropDown.PlacementContentPlaceHolderId = "1";
            //            twoColumn.AllContent.Add(layoutDropDown);

            //            var oneColumn =
            //builder.BuildCmsPageContentFromWebFormsControl(new RowLayout { NumColumns = 1 });
            //            oneColumn.PlacementContentPlaceHolderId = ConfiguratorFormBuilder.RuntimePlaceHolderId;
            //            form.FormContent.Add(oneColumn);

            //            var keywords =
            //                builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorTextBox
            //                {
            //                    PropertyName = nameof(CmsPage.Keywords),
            //                    DisplayName = "Keywords",
            //                });
            //            keywords.PlacementContentPlaceHolderId = "0";
            //            oneColumn.AllContent.Add(keywords);

            //            var description =
            //                builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorTextBox
            //                {
            //                    PropertyName = nameof(CmsPage.Description),
            //                    DisplayName = "Description",
            //                });
            //            oneColumn.AllContent.Add(description);

            //            var siteIdDropdown =
            //                builder.BuildCmsPageContentFromWebFormsControl(new ConfiguratorHiddenField()
            //                {
            //                    PropertyName = nameof(CmsPage.SiteId)
            //                });
            //            oneColumn.AllContent.Add(siteIdDropdown);

            var formRepository = new FormRepository();

            formRepository.Save(form);
            formRepository.Publish(By.ContentId(form.ContentId));

            return form;
        }

        public void SetupBackendSite()
        {

            var listRepo = new ContentListDefinitionRepository();
            var customPageList = new ConfiguratorCmsPageContentBuilder().GenerateDefaultContentListDefinition(typeof(CmsPage));
            customPageList.EntityUid = new Guid(CmsForm.ApiId);
            listRepo.Save(customPageList);
            listRepo.Publish(By.ContentId(customPageList.ContentId));




            var backendLayout = new Layout
            {
                Name = "Backend Layout",
                MasterPagePath = "/App_Data/BackendPage.Master",
                PageContent = new List<CmsPageContent> { new CmsPageContent { Id = Guid.NewGuid(), PlacementContentPlaceHolderId = "Body", WidgetTypeCode = "wc-navigation-bar" } }
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
                ContentId = KnownPageIds.FormDesigner,
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false,

            };
            formDesigner.PageContent.Add(new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-formdesigner"
            });


            var pageTree = new CmsPage
            {
                Name = "Pages",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId
            };
            
            pageTree.PageContent.AddBlazorApp(new PageTreeApp());


            var redirectPageSettingsForm = SetupRedirectPageSettingsForm();
            var groupingPageSettingsForm = SetupGroupingPageSettingsForm();
            var pageSettingsForm = SetupPageSettingsForm();


            var addPageWizard = new CmsPage
            {
                Name = "Add Page Wizard",
                ContentId = KnownPageIds.AddPageWizard,
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false
            };
            addPageWizard.PageContent.Add(new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = AddPageWizard.ApiId,

            });



            var contentPageSettings = new CmsPage
            {
                Name = "Content Page Settings",
                ContentId = KnownPageIds.ContentPageSettings,
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false
            };
            contentPageSettings.PageContent.AddDynamicForm(pageSettingsForm);


            var groupingPageSettings = new CmsPage
            {
                Name = "Group Page Settings",
                ContentId = KnownPageIds.GroupingPageSettings,
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false
            };
            groupingPageSettings.PageContent.AddDynamicForm(groupingPageSettingsForm);

            var redirectPageSettings = new CmsPage
            {
                Name = "Redirect Page Settings",
                ContentId = KnownPageIds.RedirectPageSettings,
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = false
            };
            redirectPageSettings.PageContent.AddDynamicForm(redirectPageSettingsForm);

            var dashboardPage = new CmsPage
            {
                Name = "Dashboard",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId
            };


            var entityBuilderPage = new CmsPage
            {
                Name = "Entity Builder",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId
            };
            entityBuilderPage.PageContent.Add(new CmsPageContent
            {
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = "wc-entity-list",
                Parameters = new Dictionary<string, string> { }
            });



            //////////////////
            var dynamicListTest = new CmsPage
            {
                Name = "Dynamic List Test",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                DisplayInNavigation = true
            };

            //var contentListControl = new ContentList()
            //{
            //    RepositoryId = new Guid(CmsPageRepository.ApiId),
            //    Config = new ContentListConfiguration
            //    {
            //        Fields = new List<ContentListField>
            //        {
            //            new ContentListField
            //            {
            //                Header = "Name",
            //                Template = "{" + nameof(CmsPage.Name) + "}"
            //            }

            //        }

            //    },

            //};
            //var parameters = (Dictionary<string, string>)contentListControl.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);



            //var list= new ContentListDefinitionRepository().GetRandomList(new Guid(CmsForm.ApiId));
            //var bApp = new BlazorApp
            //{
            //    StartingRouteTemplate = "",
            //    StartingRouteParameters = new RouteDataDictionary()
            //    {
            //        ["RepositoryApiId"] = FormRepository.ApiId,
            //        ["ListId"] = list.ContentId.ToString()
            //    }
            //};
            //var listContent =new InstallationHelpers().BuildCmsPageContentFromToolboxItemTemplate();
            //listContent.PlacementContentPlaceHolderId = "Body";

            var contentBrowserConfig = new ContentBrowserConfiguration();
            contentBrowserConfig.RepositoryApiId = new Guid(CmsPageRepository.ApiId);
            contentBrowserConfig.ListId = customPageList.ContentId;
            contentBrowserConfig.EditFormId = KnownFormIds.ContentPageSettingsForm;
            contentBrowserConfig.AddFormId = KnownFormIds.ContentPageSettingsForm;

            dynamicListTest.PageContent.AddBlazorApp(new ContentBrowserApp{Configuration  = contentBrowserConfig});

            //dynamicListTest.PageContent.Add(new CmsPageContent
            //{
            //    Id = Guid.NewGuid(),
            //    PlacementContentPlaceHolderId = "Body",
            //    WidgetTypeCode = ContentList.ApiId,
            //    Parameters = 
            //});


            ////////////////
            var formsList = new CmsPage
            {
                Name = "FormsLegacy",
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
                            Header = "Title",
                            Template ="{"+nameof(CmsForm.Title)+"}"
                        },
                        new ContentListField
                        {
                            Header = "Actions",
                            Template =@"<a href='/Admin/form-designer?formId={"+nameof(CmsForm.DesignForContentId)+"}'><span class='glyphicon glyphicon-text-background'></span>Design</a>"
                        }

                    },
                    AddPage = KnownPageIds.FormDesigner
                }
            };
            var formListParameters = (Dictionary<string, string>)formList.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);

            formsList.PageContent.Add(new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "Body",
                WidgetTypeCode = ContentList.ApiId,
                Parameters = formListParameters
            });



            var pageRepo = new CmsPageRepository();
            pageRepo.Save(dashboardPage);
            pageRepo.Save(pageTree);
            pageRepo.Save(formDesigner);
            pageRepo.Save(contentPageSettings);
            pageRepo.Save(groupingPageSettings);
            pageRepo.Save(redirectPageSettings);
            pageRepo.Save(addPageWizard);
            pageRepo.Save(entityBuilderPage);
            pageRepo.Save(entityListPage);
            pageRepo.Save(dynamicListTest);
            pageRepo.Save(formsList);
            backendSite.HomepageId = dashboardPage.ContentId;
            siteRepo.Save(backendSite);

            var editBackendPageTreeLink = new CmsPage
            {
                Name = "Edit Backend Pages",
                SiteId = backendSite.ContentId,
                LayoutId = backendLayout.ContentId,
                PageType = PageType.RedirectPage,
                RedirectUri = new WarpCorePageUri(pageTree),
                InternalRedirectParameters = new Dictionary<string, string> { ["SiteId"] = backendSite.ContentId.ToString() }
            };

            pageRepo.Save(editBackendPageTreeLink);




        }

        public Site SetupTestSite()
        {
            var tbx = new ToolboxManager();

            var myLayout = new Layout
            {
                Name = "Demo",
                MasterPagePath = "/Demo.Master",

            };

            myLayout.PageContent.Add(new CmsPageContent
            {
                Id = Guid.NewGuid(),
                WidgetTypeCode = "Client-CustomNavigation",
                PlacementContentPlaceHolderId = "NavigationContentPlaceHolder"
            });
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
                WidgetTypeCode = RowLayout.ApiId,
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
                WidgetTypeCode = ContentBlock.ApiId,
                Parameters = new Dictionary<string, string> { ["AdHocHtml"] = "Hello World (0)" }
            };

            var helloWorld1 = new CmsPageContent
            {
                Id = Guid.NewGuid(),
                PlacementContentPlaceHolderId = "1",
                PlacementLayoutBuilderId = lbId,
                WidgetTypeCode = ContentBlock.ApiId,
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

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {


            var vpp = EmbeddedResourceVirtualPathProviderStart.Start();
            var blazor = new BlazorModuleBuilder();

            var tooling = (typeof(BackendSiteTooling.Class1)).Assembly;
            blazor.HostBlazorModule(tooling);


            var loadingHtml = AssemblyResourceReader.ReadString(tooling, "BackendSiteTooling.Boot.loading.html");
            BootPage.RegisterHtml(loadingHtml);
        }

    }
}