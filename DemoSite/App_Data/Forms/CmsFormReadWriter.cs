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

            //var configuratorWidget = activatedWidget as IConfiguratorControl;
            //if (configuratorWidget != null)
            //{
            //    var editingContext = new ConfiguratorEditingContext();
            //    editingContext.CmsPageContent = content;
            //    editingContext.
            //        configuratorWidget.InitializeEditingContext();
            //}

        }

        public static void FillInControlValues(Control surface,ConfiguratorEditingContext editingContext)
        {
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
                tbx.Text = editingContext.CurrentValues[tbx.PropertyName];

            foreach (var tbx in surface.GetDescendantControls<ConfiguratorCheckBox>())
            {
                var rawValue = editingContext.CurrentValues[tbx.PropertyName];

                bool outValue;
                var success = Boolean.TryParse(rawValue, out outValue);
                if (success)
                    tbx.Checked = outValue;
            }

            foreach (var tbx in surface.GetDescendantControls<ConfiguratorDropDownList>())
                tbx.SelectedValue = editingContext.CurrentValues[tbx.PropertyName];


        }

        public static IDictionary<string, string> ReadValuesFromControls(Control surface)
        {
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
            {
                newParameters.Add(tbx.PropertyName, tbx.Text);
            }
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorDropDownList>())
            {
                newParameters.Add(tbx.PropertyName, tbx.SelectedValue);
            }
            return newParameters;
        }

    }
}