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
    public class CmsFormEventsDataHiddenField : HiddenField
    {
        private class CmsFormEventsData
        {
            public Dictionary<string, string> PreviousControlValues { get; set; }
            public Guid PageContentId { get; set; }
        }

        public Dictionary<string, string> PreviousControlValues
        {
            get
            {
                return GetFieldValue().PreviousControlValues;
            }
            set
            {
                var obj = GetFieldValue();
                obj.PreviousControlValues = value;
                this.Value = new JavaScriptSerializer().Serialize(obj);
            }
        }

        public Guid PageContentId
        {
            get { return GetFieldValue().PageContentId; }
            set
            {
                var obj = GetFieldValue();
                obj.PageContentId = value;
                this.Value = new JavaScriptSerializer().Serialize(obj);
            }
        }

        private CmsFormEventsData GetFieldValue()
        {
            if (string.IsNullOrWhiteSpace(Value))
                return new CmsFormEventsData();

            return new JavaScriptSerializer().Deserialize<CmsFormEventsData>(Value);
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
                        PropertyName = key,
                        Model = newValues
                    };
                }
            }
        }
        public static CmsFormEventsDataHiddenField GetEventTracking(Control surface)
        {
            return surface.Controls.OfType<CmsFormEventsDataHiddenField>().FirstOrDefault();
            
        }

        public static CmsFormEventsDataHiddenField AddEventTracking(Control surface, ConfiguratorBuildArguments buildArguments)
        {
            var formData = GetEventTracking(surface);
            if (formData != null)
                return formData;

            var previousControlValues = buildArguments.DefaultValues.ToDictionary(x => x.Key, x => x.Value);
            var pageContentId = buildArguments.PageContentId;
            formData = new CmsFormEventsDataHiddenField();
            formData.PageContentId = pageContentId;
            formData.PreviousControlValues = previousControlValues;
            surface.Controls.Add(formData);

            return formData;
        }



        public static void InitializeEditing(Control surface, ConfiguratorBuildArguments buildArguments)
        {
            var configuratorControls = surface.GetDescendantControls<Control>()
                .OfType<IConfiguratorControl>()
                .ToList();

            foreach (var configuratorControl in configuratorControls)
            {
                configuratorControl.InitializeEditingContext(buildArguments);

                if (configuratorControl.Behaviors != null)
                {

                    var behaviors = configuratorControl.Behaviors
                        .Select(Type.GetType)
                        .Select(x => Activator.CreateInstance(x))
                        .Cast<IUserInterfaceBehavior>()
                        .ToList();

                    foreach (var behavior in behaviors)
                        behavior.RegisterBehavior(configuratorControl, buildArguments);
                }
            }

            

        }

        public static void FillInControlValues(Control surface, ConfiguratorBuildArguments buildArguments)
        {
            FillInControlValues(surface,buildArguments.DefaultValues);
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