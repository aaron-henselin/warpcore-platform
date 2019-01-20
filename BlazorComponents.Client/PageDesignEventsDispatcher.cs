using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorComponents.Shared;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorComponents.Client
{
    public class ValueChangedEventArgs
    {
        public string PropertyName { get; set; }
        public string NewValue { get; set; }
    }

    public class FormEventDispatcher
    {
        public event EventHandler ValueChanged;

        public Dictionary<string, string> CurrentValues { get; set; } = new Dictionary<string, string>();
        
        public Dictionary<string, IConfiguratorComponent> RegisteredComponents { get; set; } = new Dictionary<string, IConfiguratorComponent>();

        public void RegisterComponent(string property, IConfiguratorComponent blazorComponent)
        {
            Console.WriteLine($@"Registering blazor form control {blazorComponent.Setup.EditorCode} for {property}");

            if (!RegisteredComponents.ContainsKey(property))
                RegisteredComponents.Add(property, blazorComponent);
            else
                RegisteredComponents[property] = blazorComponent;
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
