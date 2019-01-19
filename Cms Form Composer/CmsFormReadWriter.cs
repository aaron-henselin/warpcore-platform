using System;
using System.Collections.Generic;


namespace WarpCore.Web.Widgets.FormBuilder.Support
{
    //public class CmsFormEventsDataHiddenField : HiddenField
    //{
    //    private class CmsFormEventsData
    //    {
    //        public Dictionary<string, string> PreviousControlValues { get; set; }
    //        public Guid PreviousPageContentId { get; set; }

            
    //    }

    //    public Dictionary<string, string> PreviousControlValues
    //    {
    //        get
    //        {
    //            return GetFieldValue().PreviousControlValues;
    //        }
    //        set
    //        {
    //            var obj = GetFieldValue();
    //            obj.PreviousControlValues = value;
    //            this.Value = new JavaScriptSerializer().Serialize(obj);
    //        }
    //    }

    //    public Guid PageContentId { get; set; }

    //    public Guid PreviousPageContentId
    //    {
    //        get { return GetFieldValue().PreviousPageContentId; }
    //        set
    //        {
    //            var obj = GetFieldValue();
    //            obj.PreviousPageContentId = value;
    //            this.Value = new JavaScriptSerializer().Serialize(obj);
    //        }
    //    }

    //    private CmsFormEventsData GetFieldValue()
    //    {
    //        if (string.IsNullOrWhiteSpace(Value))
    //            return new CmsFormEventsData();

    //        return new JavaScriptSerializer().Deserialize<CmsFormEventsData>(Value);
    //    }

    //    public ConfiguratorEvents Events { get;  }= new ConfiguratorEvents();
    //    public IReadOnlyCollection<IConfiguratorControl> MonitoredConfigurators { get; set; }

    //    public void RaiseEvents()
    //    {
    //        var surface = this.Parent;
    //        var previousConfiguredContentId = CmsFormReadWriter.GetEventTracking(surface).PreviousPageContentId;
    //        var newConfiguredContentId = PageContentId;
    //        var isSameForm = previousConfiguredContentId == newConfiguredContentId;
    //        if (isSameForm)
    //        {
    //            var values = CmsFormReadWriter.GetChangedValues(surface,MonitoredConfigurators);
    //            foreach (var value in values)
    //                Events.RaiseValueChanged(value);

    //        }
    //    }

    //    public void UpdateEventData()
    //    {
    //        PreviousControlValues = CmsFormReadWriter.ReadValuesFromControls(MonitoredConfigurators);
    //        PreviousPageContentId = PageContentId;

    //    }
    //}

    public static class CmsFormReadWriter
    {
        //public static IEnumerable<ValueChangedEventArgs> GetChangedValues(Control surface, IReadOnlyCollection<IConfiguratorControl> monitoredConfigurators)
        //{


        //    var eventTracking = CmsFormReadWriter.GetEventTracking(surface);

        //    var newValues = CmsFormReadWriter.ReadValuesFromControls(monitoredConfigurators);
        //    foreach (var key in newValues.Keys)
        //    {
        //        var isNewValue = !string.Equals(newValues[key], eventTracking.PreviousControlValues[key]);
        //        if (isNewValue)
        //        {
        //            yield return new ValueChangedEventArgs
        //            {
        //                NewValue = newValues[key],
        //                OldValue = eventTracking.PreviousControlValues[key],
        //                PropertyName = key,
        //                Model = newValues
        //            };
        //        }
        //    }
        //}
        //public static CmsFormEventsDataHiddenField GetEventTracking(Control surface)
        //{
        //    return surface.Controls.OfType<CmsFormEventsDataHiddenField>().FirstOrDefault();
            
        //}

        //public static CmsFormEventsDataHiddenField AddEventTracking(Control surface, ConfiguratorBuildArguments buildArguments, IReadOnlyCollection<IConfiguratorControl> monitoredConfigurators)
        //{
        //    var formData = GetEventTracking(surface);
        //    if (formData != null)
        //        return formData;

        //    var previousControlValues = buildArguments.DefaultValues.ToDictionary(x => x.Key, x => x.Value);
        //    var pageContentId = buildArguments.PageContentId;
        //    formData = new CmsFormEventsDataHiddenField();
        //    formData.PageContentId =pageContentId;
        //    formData.PreviousPageContentId = pageContentId;
        //    formData.PreviousControlValues = previousControlValues;
        //    formData.MonitoredConfigurators = monitoredConfigurators;
        //    surface.Controls.Add(formData);

        //    return formData;
        //}



        //public static void InitializeEditing(IReadOnlyCollection<IConfiguratorControl> controls, ConfiguratorBuildArguments buildArguments)
        //{


        //    foreach (var configuratorControl in controls)
        //    {
        //        configuratorControl.InitializeEditingContext(buildArguments);

        //        if (configuratorControl.Behaviors != null)
        //        {

        //            var behaviors = configuratorControl.Behaviors
        //                .Select(Type.GetType)
        //                .Select(x => Activator.CreateInstance(x))
        //                .Cast<IUserInterfaceBehavior>()
        //                .ToList();

        //            foreach (var behavior in behaviors)
        //                behavior.RegisterBehavior(configuratorControl, buildArguments);
        //        }
        //    }

            

        //}

        //public static void SetDefaultValues(IReadOnlyCollection<IConfiguratorControl> controls, ConfiguratorBuildArguments buildArguments)
        //{
        //    FillInControlValues(controls,buildArguments.DefaultValues);
        //}

        public static void FillInControlValues(IReadOnlyCollection<IConfiguratorControl> controls, IDictionary<string,string> newValues)
        {
            foreach (var control in controls)
            {
                if (string.IsNullOrWhiteSpace(control.PropertyName))
                    throw new Exception("Control "+control.GetType().Name+" is not mapped to a property.");

                control.SetValue(newValues[control.PropertyName]);
            }
        }

        public static Dictionary<string, string> ReadValuesFromControls(IReadOnlyCollection<IConfiguratorControl> controls)
        {
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var control in controls)
                newParameters.Add(control.PropertyName, control.GetValue());

            return newParameters;
        }

    }
}