using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Platform.Kernel;

namespace WarpCore.Web.RenderingEngines.Mvc
{
    public class MvcCompositionElementFactory : IPageCompositionElementFactory
    {


        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new[] {typeof(Controller)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new[] {KnownPhysicalFileExtensions.Razor};
        }

        public PageCompositionElement CreateRenderingForObject(object nativeWidgetObject)
        {
            return new ControllerPageCompositionElement((IController)nativeWidgetObject);
        }

        public PageCompositionElement CreateRenderingForPhysicalFile(string physicalFilePath)
        {
            throw new NotImplementedException();
        }

        public object ActivateType(Type type)
        {
            Controller c;
            var activator= DependencyResolver.Current.GetService<IControllerActivator>();
            if (activator != null)
                c = (Controller)activator.Create(null, type);
            else
            {
                c = (Controller)DependencyResolver.Current.GetService(type);
            }

            c.ViewEngineCollection.Insert(0,new EmbeddedResourceViewEngine(type.Assembly));

            return c;
        }
    }
    public class EmbeddedResourceViewEngine : RazorViewEngine
    {
        private readonly Assembly _asm;

        public EmbeddedResourceViewEngine(Assembly asm)
        {
            _asm = asm;
            var asmName = asm.FullName;
            foreach (var character in Path.GetInvalidFileNameChars())
                asmName = asmName.Replace(character, '_');

            this.AreaViewLocationFormats = new string[4]
            {
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/{{1}}/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/{{1}}/{{0}}.vbhtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/Shared/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/Shared/{{0}}.vbhtml"
            };
            this.AreaMasterLocationFormats = new string[4]
            {
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/{{1}}/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/{{1}}/{{0}}.vbhtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/Shared/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/Shared/{{0}}.vbhtml"
            };
            this.AreaPartialViewLocationFormats = new string[4]
            {
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/{{1}}/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/{{1}}/{{0}}.vbhtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/Shared/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Areas/{{2}}/Views/Shared/{{0}}.vbhtml"
            };
            this.ViewLocationFormats = new string[4]
            {
                $"~/App_Data/EmbeddedResources/{asmName}/Views/{{1}}/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/{{1}}/{{0}}.vbhtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/Shared/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/Shared/{{0}}.vbhtml"
            };
            this.MasterLocationFormats = new string[4]
            {
                $"~/App_Data/EmbeddedResources/{asmName}/Views/{{1}}/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/{{1}}/{{0}}.vbhtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/Shared/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/Shared/{{0}}.vbhtml"
            };
            this.PartialViewLocationFormats = new string[4]
            {
                $"~/App_Data/EmbeddedResources/{asmName}/Views/{{1}}/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/{{1}}/{{0}}.vbhtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/Shared/{{0}}.cshtml",
                $"~/App_Data/EmbeddedResources/{asmName}/Views/Shared/{{0}}.vbhtml"
            };
            this.FileExtensions = new string[2]
            {
                "cshtml",
                "vbhtml"
            };


        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            var asm = controllerContext.Controller.GetType().Assembly;
            if (asm != _asm)
                return new ViewEngineResult(new List<string>());

            return base.FindView(controllerContext, viewName, masterName, useCache);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            var asm = controllerContext.Controller.GetType().Assembly;
            if (asm != _asm)
                return new ViewEngineResult(new List<string>());

            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }
    }
}