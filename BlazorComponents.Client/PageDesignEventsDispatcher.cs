using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorComponents.Shared;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorComponents.Client
{
    public class StartEditingArgs : EventArgs
    {
        public FormSession Session { get; set; }
    }

    public class FormSession
    {
        public Guid ContentId { get; set; }
        public Dictionary<string,string> OriginalValues { get; set; }
    }

    public class ValueChangedEventArgs
    {
        public string PropertyName { get; set; }
        public string NewValue { get; set; }
    }

    public class FormEventDispatcher
    {
        public event EventHandler ValueChanged;
        //public event EventHandler<StartEditingArgs> SessionStarted;
        public event EventHandler EditingCancelled;
        
        
        public Dictionary<string, IConfiguratorComponent> RegisteredComponents { get; set; } = new Dictionary<string, IConfiguratorComponent>();

        public void RegisterComponent(string property, IConfiguratorComponent blazorComponent)
        {
            Console.WriteLine($@"[Forms] Registering blazor form control {blazorComponent.Setup.EditorCode} for {property}");

            if (!RegisteredComponents.ContainsKey(property))
                RegisteredComponents.Add(property, blazorComponent);
            else
                RegisteredComponents[property] = blazorComponent;
        }

        public void StartNewSession(FormSession session)
        {
            SetControlValuesFromOriginalValues(session);
          //  SessionStarted?.Invoke(this, new StartEditingArgs { Session = session });
        }

        private void SetControlValuesFromOriginalValues(FormSession session)
        {
            var propertyNames = RegisteredComponents.Keys;
            foreach (var propertyName in propertyNames)
            {
                if (session.OriginalValues.ContainsKey(propertyName))
                {
                    var originalValue = session.OriginalValues[propertyName];
                    Console.WriteLine($"[Forms] Setting {propertyName} to original value '{originalValue}'");
                    RegisteredComponents[propertyName].Value = originalValue;
                }
                else
                {
                    Console.WriteLine($"[Forms] No value is available for component {propertyName}.");
                    RegisteredComponents[propertyName].Value = null;
                }
            }
        }

        public void CancelEditingSession()
        {
            EditingCancelled?.Invoke(this,new EventArgs());
        }

        public void RaiseValueChanged(string property)
        {

        }
    }

    public class PageDesignEventsDispatcher
    {
        public Action<Guid> Edit { get; set; }

        public Action<Guid> Delete { get; set; }
    }

    public class DesignerChrome
    {
        public SideBarMode SideBarMode { get; set; }

        public static DesignerChrome Default => new DesignerChrome();

    }

    public enum SideBarMode { Collapsed, Toolbox, Configurator}


}
