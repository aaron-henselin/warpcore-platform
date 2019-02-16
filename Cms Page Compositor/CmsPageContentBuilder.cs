using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.PageComposition
{
    //todo: is this just a test feature?
    public class CmsPageContentBuilder
    {
        private readonly CmsPageContentActivator _activator;

        public CmsPageContentBuilder():this(Dependency.Resolve<CmsPageContentActivator>())
        {
        }

        public CmsPageContentBuilder(CmsPageContentActivator activator) 
        {
            _activator = activator;
        }

        public CmsPageContent BuildCmsPageContentFromTemplate(object activated) 
        {
            var toolboxMetadata = ToolboxMetadataReader.ReadMetadata(activated.GetType());
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(toolboxMetadata.WidgetUid);

            IDictionary<string, string> settings;

            if (activated != null)
                settings = activated.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);
            else
                settings = _activator.GetDefaultContentParameterValues(toolboxItem);

            return new CmsPageContent
            {

                Id = Guid.NewGuid(),
                WidgetTypeCode = toolboxItem.WidgetUid,
                Parameters = settings.ToDictionary(x => x.Key, x => x.Value)
                    
            };
        }

       

    }
}