using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Forms;
using WarpCore.Web.Extensions;

namespace WarpCore.Web.Widgets.FormBuilder.Support
{
    public class CmsFormEventsData : HiddenField
    {

        public IDictionary<string,string> PreviousControlValues {
            get
            {
                return new JavaScriptSerializer().Deserialize<Dictionary<string,string>>(Value);
            }
            set
            {
                this.Value = new JavaScriptSerializer().Serialize(value);
            }
        } 
    }

    public static class CmsFormReadWriter
    {
        public static IEnumerable<ValueChangedEventArgs> GetChangedValues(Control surface)
        {
            var eventTracking = CmsFormReadWriter.GetEventTracking(surface);

            var newValues = CmsFormReadWriter.ReadValuesFromControls(surface);
            foreach (var key in newValues.Keys)
            {
                var isNewValue = !string.Equals(newValues[key], eventTracking.PreviousControlValues[key]);
                if (isNewValue)
                {
                    yield return new ValueChangedEventArgs
                    {
                        NewValue = newValues[key],
                        OldValue = eventTracking.PreviousControlValues[key],
                        PropertyName = key
                    };
                }
            }
        }
        public static CmsFormEventsData GetEventTracking(Control surface)
        {
            return surface.Controls.OfType<CmsFormEventsData>().FirstOrDefault();
            
        }

        public static CmsFormEventsData AddEventTracking(Control surface, ConfiguratorEditingContext editingContext)
        {
            var formData = GetEventTracking(surface);
            if (formData != null)
                return formData;
            

            formData = new CmsFormEventsData();
            formData.PreviousControlValues = editingContext.CurrentValues;
            surface.Controls.Add(formData);

            return formData;
        }



        public static void PopulateListControls(Control surface, ConfiguratorEditingContext editingContext)
        {
            var configuratorControls = surface.GetDescendantControls<Control>()
                .OfType<IConfiguratorControl>()
                .ToList();

            foreach (var configuratorControl in configuratorControls)
            {
                configuratorControl.InitializeEditingContext(editingContext);

                var behaviors = configuratorControl.Behaviors
                    .Select(Type.GetType)
                    .Select(x => Activator.CreateInstance(x))
                    .Cast<IUserInterfaceBehavior>()
                    .ToList();
                
                foreach (var behavior in behaviors)
                    behavior.RegisterBehavior(configuratorControl,editingContext);
            }

            

        }

        public static void FillInControlValues(Control surface, ConfiguratorEditingContext editingContext)
        {
            FillInControlValues(surface,editingContext.CurrentValues);
        }

        public static void FillInControlValues(Control surface, IDictionary<string,string> newValues)
        {
            var rt= surface.GetDescendantControls<RuntimeContentPlaceHolder>().First();
            foreach (var control in rt.Controls.OfType<IConfiguratorControl>())
            {
                control.SetValue(newValues[control.PropertyName]);
            }
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