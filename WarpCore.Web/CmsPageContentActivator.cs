using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;
using WarpCore.Web;
using WarpCore.Web.Widgets;
using File = System.IO.File;

namespace Cms
{
    public class CmsPageContentFactory
    {
        public CmsPageContent CreateToolboxItemContent(Control activated) 
        {
            var toolboxMetadata = ToolboxMetadataReader.ReadMetadata(activated.GetType());
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(toolboxMetadata.WidgetUid);

            IDictionary<string, string> settings;

            if (activated != null)
                settings = activated.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);
            else
                settings = CmsPageContentActivator.GetDefaultContentParameterValues(toolboxItem);

            return new CmsPageContent
                {

                    Id = Guid.NewGuid(),
                    WidgetTypeCode = toolboxItem.WidgetUid,
                    Parameters = settings.ToDictionary(x => x.Key, x => x.Value)
                    
                };
        }

       

    }


    public class CmsPageContentActivator
    {
        private Dictionary<string, IPartialPageRenderingFactory> _extensionLookup;
        private Dictionary<Type, IPartialPageRenderingFactory> _baseTypeLookup;


        public CmsPageContentActivator():this(Dependency.ResolveMultiple<IPartialPageRenderingFactory>())
        {
        }

        public CmsPageContentActivator(IEnumerable<IPartialPageRenderingFactory> renderingFactories)
        {
            _extensionLookup = new Dictionary<string, IPartialPageRenderingFactory>(StringComparer.OrdinalIgnoreCase);
            _baseTypeLookup = new Dictionary<Type, IPartialPageRenderingFactory>();
            foreach (var fac in renderingFactories)
            {
                var extensions = fac.GetHandledFileExtensions();
                foreach (var extension in extensions)
                {
                    if (_extensionLookup.ContainsKey(extension))
                        throw new Exception(
                            $"Extension {extension} is already handled by {_extensionLookup[extension].GetType().Name}");

                    _extensionLookup.Add(extension,fac);
                }

                var baseTypes = fac.GetHandledBaseTypes();
                foreach (var baseType in baseTypes)
                {
                    if (_baseTypeLookup.ContainsKey(baseType))
                        throw new Exception(
                            $"Base type {baseType.Name} is already handled by {_baseTypeLookup[baseType].GetType().Name}");

                    _baseTypeLookup.Add(baseType, fac);
                }
            }


        }

        public  PartialPageRendering ActivateLayoutByExtension(string virtualPath)
        {

            if (!virtualPath.Contains("."))
                throw new Exception($"Layout file does not have an extension, so the rendering engine cannot be determined.");

            var last = virtualPath.Split('.').Last();

            if (!_extensionLookup.ContainsKey(last))
                throw new Exception("No rendering engine is registered that handles file extension: " + virtualPath);

            return _extensionLookup[last].CreateRenderingForPhysicalFile(virtualPath);

        }

        public PartialPageRendering ActivateCmsPageContent(CmsPageContent pageContent)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            var activatedObject =  ActivateToolboxItemType(toolboxItem, pageContent.Parameters);
            //this is necessary, because the configuration (not the constructor) is allowed to dictate how many placeholders to create.
            //todo: move this into the partial page rendering??
            (activatedObject as ILayoutControl)?.InitializeLayout();

            var handler = _baseTypeLookup.Keys.SingleOrDefault(x => x.IsInstanceOfType(activatedObject));

            if (handler == null)
                throw new Exception($"Rendering engine able to handle {activatedObject.GetType().Name} was not found.");

            var pp = _baseTypeLookup[handler].CreateRenderingForObject(activatedObject);
            pp.ContentId = pageContent.Id;
            pp.FriendlyName = toolboxItem.FriendlyName;
            pp.LocalId = $"Layout{pageContent.Id}";
            pp.LayoutBuilderId = pageContent.Id;

            return pp;
        }

        public static object ActivateToolboxItemType(ToolboxItem toolboxItem, IDictionary<string,string> parameters)
        {
            var toolboxItemType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);
            var activatedWidget = (object)Activator.CreateInstance(toolboxItemType);
            activatedWidget.SetPropertyValues(parameters, ToolboxPropertyFilter.SupportsDesigner);

            return activatedWidget;
        }

        public static IDictionary<string, string> GetDefaultContentParameterValues(ToolboxItem toolboxItem)
        {
            
            var activated = ActivateToolboxItemType(toolboxItem, new Dictionary<string, string>());
            return activated.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);
        }

        

        //public static void SetContentParameterValues(Control activatedWidget, IDictionary<string,string> parameterValues)
        //{
        //    if (parameterValues == null)
        //        parameterValues = new Dictionary<string, string>();

        //    foreach (var kvp in parameterValues)
        //    {
        //        var propertyInfo = activatedWidget.GetType().GetProperty(kvp.Key);
        //        if (propertyInfo == null || !propertyInfo.CanWrite)
        //            continue;

        //        var isSetting = propertyInfo.GetCustomAttribute<UserInterfaceHintAttribute>() != null;
        //        if (!isSetting)
        //            continue;

        //        try
        //        {
        //            var newType = DesignerTypeConverter.ChangeType(kvp.Value, propertyInfo.PropertyType);
        //            if (propertyInfo.CanWrite)
        //                propertyInfo.SetValue(activatedWidget, newType);
        //        }
        //        catch (Exception e)
        //        {
                    
        //        }
                

        //    }
            
        //}
    }
}