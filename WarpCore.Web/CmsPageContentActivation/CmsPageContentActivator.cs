using System;
using System.Collections.Generic;
using System.Web.UI;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;

namespace WarpCore.Web
{


    public class CmsPageContentActivator
    {


        public static Control ActivateControl(CmsPageContent pageContent)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(pageContent.WidgetTypeCode);
            var toolboxItemType = Type.GetType(toolboxItem.FullyQualifiedTypeName);
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

                var newType = Convert.ChangeType(kvp.Value, propertyInfo.PropertyType);
                if (propertyInfo.CanWrite)
                    propertyInfo.SetValue(activatedWidget, newType);
            }
            
        }
    }
}