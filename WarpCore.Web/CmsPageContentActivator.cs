using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;

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


        public static Control ActivateControl(CmsPageContent pageContent)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            return ActivateControl(toolboxItem, pageContent.Parameters);
        }

        public static Control ActivateControl(ToolboxItem toolboxItem, IDictionary<string,string> parameters)
        {
            var toolboxItemType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);
            var activatedWidget = (Control)Activator.CreateInstance(toolboxItemType);
            activatedWidget.SetPropertyValues(parameters, ToolboxPropertyFilter.SupportsDesigner);

            return activatedWidget;
        }

        public static IDictionary<string, string> GetDefaultContentParameterValues(ToolboxItem toolboxItem)
        {
            
            var activated = ActivateControl(toolboxItem, new Dictionary<string, string>());
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

        //        var isSetting = propertyInfo.GetCustomAttribute<SettingAttribute>() != null;
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