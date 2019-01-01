using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace WarpCore.Web.RenderingEngines.Mvc
{
    public class MvcRenderingEngine : IPartialPageRenderingFactory
    {


        public IReadOnlyCollection<Type> GetHandledBaseTypes()
        {
            return new[] {typeof(Controller)};
        }

        public IReadOnlyCollection<string> GetHandledFileExtensions()
        {
            return new[] {KnownPhysicalFileExtensions.Razor};
        }

        public PartialPageRendering CreateRenderingForObject(object nativeWidgetObject)
        {
            return new ControllerPartialPageRendering((IController)nativeWidgetObject);
        }

        public PartialPageRendering CreateRenderingForPhysicalFile(string physicalFilePath)
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