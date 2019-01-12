using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Cms.Features.Presentation.Page.Elements;
using Modules.Cms.Features.Presentation.RenderingEngines.CachedContent;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
{


  public class CmsPageContentActivator
    {
        private readonly CachedContentActivator _contentContentCacheElementFactory;

        private Dictionary<string, IPageCompositionElementFactory> _extensionLookup;
        private Dictionary<Type, IPageCompositionElementFactory> _baseTypeLookup;


        public CmsPageContentActivator():this(Dependency.ResolveMultiple<IPageCompositionElementFactory>(), Dependency.Resolve<CachedContentActivator>())
        {
        }

        public CmsPageContentActivator(IEnumerable<IPageCompositionElementFactory> renderingFactories, CachedContentActivator contentContentCacheElementFactory)
        {
            _contentContentCacheElementFactory = contentContentCacheElementFactory;
            _extensionLookup = new Dictionary<string, IPageCompositionElementFactory>(StringComparer.OrdinalIgnoreCase);
            _baseTypeLookup = new Dictionary<Type, IPageCompositionElementFactory>();
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
        

        public  Type CompileResourceAtVirtualPath(string virtualPath)
        {

            if (!virtualPath.Contains("."))
                throw new Exception($"Layout file does not have an extension, so the rendering engine cannot be determined.");

            var last = virtualPath.Split('.').Last();

            if (!_extensionLookup.ContainsKey(last))
                throw new Exception("No rendering engine is registered that handles file extension: " + virtualPath);

            
            return _extensionLookup[last].Build(virtualPath);

        }

        public PageCompositionElement ActivateRootLayout(string layoutFileName)
        {
            var toolboxItemType = CompileResourceAtVirtualPath(layoutFileName);

            PageCompositionElement pp;
            var wasCreatedViaCache = _contentContentCacheElementFactory.TryCreateCachedContentElement(toolboxItemType, new CmsPageContent
                {Id= SpecialRenderingFragmentContentIds.PageRoot }, out pp, out var cacheKey);
            if (!wasCreatedViaCache)
            {
                var activator = GetActivator(toolboxItemType);
                //var activatedObject = activator.ActivateType(toolboxItemType);
                //activatedObject.SetPropertyValues(parameters, ToolboxPropertyFilter.SupportsDesigner);
                pp = activator.CreateRenderingForObject(layoutFileName);
            }

            pp.CacheKey = cacheKey;
            pp.ContentId = SpecialRenderingFragmentContentIds.PageRoot;
            pp.FriendlyName = "Page Layout";
            pp.LocalId = $"Layout";
            pp.LayoutBuilderId = Guid.Empty;

            return pp;
        }

        public PageCompositionElement ActivateCmsPageContent(CmsPageContent pageContent)
        {
            var parameters = pageContent.Parameters;
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);

            Type toolboxItemType;
            if (toolboxItem.AscxPath == null)
                toolboxItemType = Type.GetType(toolboxItem.AssemblyQualifiedTypeName);
            else
                toolboxItemType=CompileResourceAtVirtualPath(toolboxItem.AscxPath);
            

            PageCompositionElement pp;
            var wasCreatedViaCache =_contentContentCacheElementFactory.TryCreateCachedContentElement(toolboxItemType, pageContent, out pp, out var cacheKey);
            if (!wasCreatedViaCache)
            {
                var activator = GetActivator(toolboxItemType);
                var activatedObject = activator.ActivateType(toolboxItemType);
                activatedObject.SetPropertyValues(parameters, ToolboxPropertyFilter.SupportsDesigner);
                pp = activator.CreateRenderingForObject(activatedObject);
            }

            pp.CacheKey = cacheKey;
            pp.ContentId = pageContent.Id;
            pp.FriendlyName = toolboxItem.FriendlyName;
            pp.LocalId = $"Layout{pageContent.Id}";
            pp.LayoutBuilderId = pageContent.Id;

            return pp;
        }

        private IPageCompositionElementFactory GetActivator(Type toolboxItemType)
        {
            var handler = _baseTypeLookup.Keys.SingleOrDefault(x => x.IsAssignableFrom(toolboxItemType));
            if (handler == null)
                throw new Exception($"Rendering engine able to handle {toolboxItemType.Name} was not found.");

            var activator = _baseTypeLookup[handler];
            return activator;
        }


        public IDictionary<string, string> GetDefaultContentParameterValues(ToolboxItem toolboxItem)
        {
           return new CmsPageContentActivator().GetDefaultContentParameterValues(toolboxItem);
        }




    }
}