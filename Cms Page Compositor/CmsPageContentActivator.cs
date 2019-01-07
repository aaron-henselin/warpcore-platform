using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using Cms.Toolbox;
using Modules.Cms.Features.Presentation.PageComposition.Cache;
using Modules.Cms.Features.Presentation.PageComposition.Elements;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;
using WarpCore.Web;

namespace Modules.Cms.Features.Presentation.PageComposition
{


  public class CmsPageContentActivator
    {
        private readonly CmsPageContentCacheResolver _cacheResolver;
        private Dictionary<string, IPageCompositionElementFactory> _extensionLookup;
        private Dictionary<Type, IPageCompositionElementFactory> _baseTypeLookup;


        public CmsPageContentActivator():this(Dependency.ResolveMultiple<IPageCompositionElementFactory>(), Dependency.Resolve<CmsPageContentCacheResolver>())
        {
        }

        public CmsPageContentActivator(IEnumerable<IPageCompositionElementFactory> renderingFactories, CmsPageContentCacheResolver cacheResolver)
        {
            _cacheResolver = cacheResolver;
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

        public  PageCompositionElement ActivateLayoutByExtension(string virtualPath)
        {

            if (!virtualPath.Contains("."))
                throw new Exception($"Layout file does not have an extension, so the rendering engine cannot be determined.");

            var last = virtualPath.Split('.').Last();

            if (!_extensionLookup.ContainsKey(last))
                throw new Exception("No rendering engine is registered that handles file extension: " + virtualPath);

            return _extensionLookup[last].CreateRenderingForPhysicalFile(virtualPath);

        }

        public PageCompositionElement ActivateCmsPageContent(CmsPageContent pageContent)
        {
            var parameters = pageContent.Parameters;
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            var toolboxItemType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);

          
            PageCompositionElement pp;
            CmsPageContentCache found=null;
            var cacheKeyParts = new CacheKeyParts{ContentId = pageContent.Id, Parameters = pageContent.Parameters, WidgetType = toolboxItemType};
            var isCacheable = _cacheResolver.IsCacheable(toolboxItemType);
            string cacheKey=null;
            if (isCacheable)
                cacheKey = _cacheResolver.GetCacheKey(cacheKeyParts);

            var canBeActivatedViaCache = isCacheable && _cacheResolver.TryResolveFromCache(cacheKey, out found);
            if (!canBeActivatedViaCache)
            {
                var activator = GetActivator(toolboxItemType);
                var activatedObject = activator.ActivateType(toolboxItemType);
                activatedObject.SetPropertyValues(parameters, ToolboxPropertyFilter.SupportsDesigner);
                pp = activator.CreateRenderingForObject(activatedObject);
            }
            else
            {
                pp = new CacheElement(found);
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
            var toolboxItemType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);
            var activator = GetActivator(toolboxItemType);
            var activatedObject = activator.ActivateType(toolboxItemType);

            return activatedObject.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);
        }




    }
}