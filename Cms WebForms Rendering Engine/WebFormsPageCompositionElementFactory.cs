using System;
using System.Collections.Generic;
using System.Web.Compilation;
using System.Web.UI;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.Kernel;
namespace Modules.Cms.Features.Presentation.RenderingEngines.WebForms
{
    public class WebFormsPageCompositionElementFactory : IPageCompositionElementFactory
    {
        public object ActivateType(Type type)
        {
            //allows us to pass in request context and such through to webforms controls.
            //I'm not sure that it really helps me though, because we're still tied to httpcontext for everything.
            return Dependency.Resolve(type);
        }

        public Type Build(string virtualPath)
        {
            return BuildManager.GetCompiledType(virtualPath);
        }

        public PageCompositionElement CreateRenderingForObject(object nativeWidgetObject)
        {
            if (nativeWidgetObject is string)
                return CreateRenderingForPhysicalFile((string)nativeWidgetObject);

            if (nativeWidgetObject is System.Web.UI.Page)
                return new WebFormsPageCompositionElement((System.Web.UI.Page)nativeWidgetObject) { ContentId = SpecialRenderingFragmentContentIds.PageRoot };


            return new WebFormsControlPageCompositionElement((Control) nativeWidgetObject);
        }

        public PageCompositionElement CreateRenderingForPhysicalFile(string physicalFilePath)
        {
            var isMaster = physicalFilePath.EndsWith(".master", StringComparison.OrdinalIgnoreCase);
            if (isMaster)
            {
                var vPath = "/App_Data/DynamicPage.aspx";
                System.Web.UI.Page page =
                    BuildManager.CreateInstanceFromVirtualPath("/App_Data/DynamicPage.aspx", typeof(System.Web.UI.Page))
                        as System.Web.UI.Page;
                page.MasterPageFile = physicalFilePath;

                return new WebFormsPageCompositionElement(page) { ContentId = SpecialRenderingFragmentContentIds.PageRoot };
            }
            var isUserControl = physicalFilePath.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase);
            if (isUserControl)
            {
                var activated = (UserControl)BuildManager.CreateInstanceFromVirtualPath(physicalFilePath, typeof(UserControl));
                return new WebFormsControlPageCompositionElement(activated);
            }

            throw new ArgumentException();
        }

        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new []{typeof(Control)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new[] {KnownPhysicalFileExtensions.MasterPage,KnownPhysicalFileExtensions.UserControl};
        }
    }
}