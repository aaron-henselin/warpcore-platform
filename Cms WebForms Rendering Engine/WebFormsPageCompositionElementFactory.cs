using System;
using System.Collections.Generic;
using System.Web.Compilation;
using System.Web.UI;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.Kernel;
using WarpCore.Web;

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

        public PageCompositionElement CreateRenderingForObject(object nativeWidgetObject)
        {
            return new WebFormsControlPageCompositionElement((Control) nativeWidgetObject);
        }

        public PageCompositionElement CreateRenderingForPhysicalFile(string physicalFilePath)
        {
            var vPath = "/App_Data/DynamicPage.aspx";
            Page page = BuildManager.CreateInstanceFromVirtualPath("/App_Data/DynamicPage.aspx", typeof(Page)) as Page;
            page.MasterPageFile = physicalFilePath;
            
            return new WebFormsPageCompositionElement(page){ContentId = SpecialRenderingFragmentContentIds.PageRoot};
        }

        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new []{typeof(Control)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new[] {KnownPhysicalFileExtensions.MasterPage};
        }
    }
}