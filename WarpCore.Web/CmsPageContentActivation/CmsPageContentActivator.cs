using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using System.Web.UI;
using Framework;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;

namespace WarpCore.Web
{


    public class CmsPageContentActivator
    {
        public static Type ResolveToolboxItemType(ToolboxItem toolboxItem)
        {
            if (!string.IsNullOrWhiteSpace(toolboxItem.FullyQualifiedTypeName))
                return Type.GetType(toolboxItem.FullyQualifiedTypeName);

            return BuildManager.GetCompiledType(toolboxItem.AscxPath);
        }

        public static Control ActivateControl(CmsPageContent pageContent)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            var toolboxItemType = ResolveToolboxItemType(toolboxItem);
            var activatedWidget = (Control)Activator.CreateInstance(toolboxItemType);
            PropertySet(activatedWidget,pageContent.Parameters);

            return activatedWidget;
        }

        

        public static void PropertySet(Control activatedWidget, Dictionary<string,string> parameterValues)
        {
            foreach (var kvp in parameterValues)
            {
                var propertyInfo = activatedWidget.GetType().GetProperty(kvp.Key);
                if (propertyInfo == null || !propertyInfo.CanWrite)
                    continue;

                var newType = DesignerTypeConverter.ChangeType(kvp.Value, propertyInfo.PropertyType);
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(activatedWidget, newType);
            }
            
        }
    }
}