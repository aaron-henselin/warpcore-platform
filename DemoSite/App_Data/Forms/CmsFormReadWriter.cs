using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.UI;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets.FormBuilder;

namespace DemoSite
{
    public static class CmsFormReadWriter
    {
        public static void PopulateListControls(Control surface, ConfiguratorEditingContext editingContext)
        {
            var configuratorControls = surface.GetDescendantControls<Control>()
                                                .OfType<IConfiguratorControl>()
                                                .ToList();

            foreach (var configuratorControl in configuratorControls)
            {
                configuratorControl.InitializeEditingContext(editingContext);
            }
        }

        public static void FillInControlValues(Control surface,ConfiguratorEditingContext editingContext)
        {
            foreach (var control in surface.GetDescendantControls<Control>().OfType<IConfiguratorControl>())
                control.SetValue(editingContext.CurrentValues[control.PropertyName]);
        }

        public static IDictionary<string, string> ReadValuesFromControls(Control surface)
        {
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var control in surface.GetDescendantControls<Control>().OfType<IConfiguratorControl>())
                newParameters.Add(control.PropertyName, control.GetValue());

            return newParameters;
        }

    }
}