using System.Collections.Generic;
using System.Web.UI;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public static class CmsFormReadWriter
    {
        public static void FillInControlValues(Control surface, IDictionary<string,string> entityValues)
        {
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
                tbx.Value = entityValues[tbx.PropertyName];
        }

        public static IDictionary<string, string> ReadValuesFromControls(Control surface)
        {
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
            {
                newParameters.Add(tbx.PropertyName, tbx.Value);
            }
            return newParameters;
        }

    }
}