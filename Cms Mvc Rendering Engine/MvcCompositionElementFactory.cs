using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

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
            
            var activator=
                DependencyResolver.Current.GetService<IControllerActivator>();
            var controller = (Controller)activator.Create(null, type);

            //todo: eventually.
            //controller.ViewEngineCollection.Insert(0,);

            return controller;
        }
    }

}