using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using WarpCore.Web.Extensions;

namespace WarpCore.Web.Widgets.FormBuilder.Support
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

        public static void FillInControlValues(Control surface, ConfiguratorEditingContext editingContext)
        {
            FillInControlValues(surface,editingContext.CurrentValues);
        }

        public static void FillInControlValues(Control surface, IDictionary<string,string> newValues)
        {
            foreach (var control in surface.GetDescendantControls<Control>().OfType<IConfiguratorControl>())
                control.SetValue(newValues[control.PropertyName]);
        }

        public static Dictionary<string, string> ReadValuesFromControls(Control surface)
        {
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var control in surface.GetDescendantControls<Control>().OfType<IConfiguratorControl>())
                newParameters.Add(control.PropertyName, control.GetValue());

            return newParameters;
        }

    }
}