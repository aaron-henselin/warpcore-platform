using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Web.EmbeddedResourceVirtualPathProvider;

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

        public Type Build(string virtualPath)
        {
            throw new NotImplementedException();
        }
    }
    public class EmbeddedResourceViewEngine : RazorViewEngine
    {
        private readonly Assembly _asm;


        private IEnumerable<string> CreateResourcePaths(IEnumerable<string> formats, IEnumerable<string> locatorPaths)
        {
            foreach (var format in formats)
            foreach (var path in locatorPaths)
                yield return path + format;
        }

        public EmbeddedResourceViewEngine(Assembly asm)
        {
            _asm = asm;

            var controllerTypes = asm.GetTypes().Where(x => typeof(IController).IsAssignableFrom(x));
      
            Dictionary<string,string> namespaceFolders = new Dictionary<string, string>();
            foreach (var type in controllerTypes)
            {
                if (!namespaceFolders.ContainsKey(type.Namespace))
                    namespaceFolders.Add(type.Namespace,EmbeddedResourcePathFactory.CreateMvcViewLocatorPath(type));
            }

            var viewLocatorPrefix = namespaceFolders.Select(x => x.Value).ToList();



            var areaViewLocationFormats = new string[4]
            {
                "Areas.{2}.Views.{1}.{0}.cshtml",
                "Areas.{2}.Views.{1}.{0}.vbhtml",
                "Areas.{2}.Views.Shared.{0}.cshtml",
                "Areas.{2}.Views.Shared.{0}.vbhtml"
            };
            AreaViewLocationFormats = CreateResourcePaths(areaViewLocationFormats, viewLocatorPrefix).ToArray();

            var areaMasterLocationFormats = new string[4]
            {
                "Areas.{2}.Views.{1}.{0}.cshtml",
                "Areas.{2}.Views.{1}.{0}.vbhtml",
                "Areas.{2}.Views.Shared.{0}.cshtml",
                "Areas.{2}.Views.Shared.{0}.vbhtml"
            };
            AreaMasterLocationFormats = CreateResourcePaths(areaMasterLocationFormats, viewLocatorPrefix).ToArray();


            var areaPartialViewLocationFormats = new string[4]
            {
                "Areas.{2}.Views.{1}.{0}.cshtml",
                "Areas.{2}.Views.{1}.{0}.vbhtml",
                "Areas.{2}.Views.Shared.{0}.cshtml",
                "Areas.{2}.Views.Shared.{0}.vbhtml"
            };
            AreaPartialViewLocationFormats = CreateResourcePaths(areaPartialViewLocationFormats, viewLocatorPrefix).ToArray();


            var viewLocationFormats = new string[4]
            {
                "Views.{1}.{0}.cshtml",
                "Views.{1}.{0}.vbhtml",
                "Views.Shared.{0}.cshtml",
                "Views.Shared.{0}.vbhtml"
            };
            ViewLocationFormats = CreateResourcePaths(viewLocationFormats, viewLocatorPrefix).ToArray();


            var masterLocationFormats = new string[4]
            {
                "Views.{1}.{0}.cshtml",
                "Views.{1}.{0}.vbhtml",
                "Views.Shared.{0}.cshtml",
                "Views.Shared.{0}.vbhtml"
            };
            MasterLocationFormats = CreateResourcePaths(masterLocationFormats, viewLocatorPrefix).ToArray();


            var partialViewLocationFormats = new string[4]
            {
                "Views.{1}.{0}.cshtml",
                "Views.{1}.{0}.vbhtml",
                "Views.Shared.{0}.cshtml",
                "Views.Shared.{0}.vbhtml"
            };
            PartialViewLocationFormats = CreateResourcePaths(partialViewLocationFormats, viewLocatorPrefix).ToArray();


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