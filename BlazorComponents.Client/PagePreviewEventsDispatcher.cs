using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorComponents.Shared;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorComponents.Client
{
    public class CommittedChangesArgs : EventArgs
    {
        public IDictionary<string,string> NewValues { get; set; }
        public Guid ContentId { get; set; }
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

    public class ConfiguratorRegistry
    {
        public Dictionary<string, IConfiguratorComponent> RegisteredComponents { get; set; } = new Dictionary<string, IConfiguratorComponent>();

        public void RegisterComponent(string propertyName, IConfiguratorComponent blazorComponent)
        {
            Console.WriteLine($@"[Forms] Registering blazor form control {blazorComponent.Setup.EditorCode} for {propertyName}");

            if (!RegisteredComponents.ContainsKey(propertyName))
                RegisteredComponents.Add(propertyName, blazorComponent);
            else
                RegisteredComponents[propertyName] = blazorComponent;
        }

        public IReadOnlyCollection<string> GetManagedPropertyNames()
        {
            return RegisteredComponents.Keys;
        }

        public IConfiguratorComponent GetConfiguratorByPropertyName(string propertyName)
        {
            return RegisteredComponents[propertyName];
        }
    }

    public class CmsFormReadWriter
    {
        private readonly ConfiguratorRegistry _registry;

        public CmsFormReadWriter(ConfiguratorRegistry registry)
        {
            _registry = registry;
        }

        public void SetValues(IDictionary<string,string> newValues)
        {
            var propertyNames = _registry.GetManagedPropertyNames();
            foreach (var propertyName in propertyNames)
            {
                if (newValues.ContainsKey(propertyName))
                {
                    var originalValue = newValues[propertyName];
                    Console.WriteLine($"[Forms] Setting {propertyName} to original value '{originalValue}'");
                    _registry.GetConfiguratorByPropertyName(propertyName).Value = originalValue;
                }
                else
                {
                    Console.WriteLine($"[Forms] No value is available for component {propertyName}.");
                    _registry.GetConfiguratorByPropertyName(propertyName).Value = null;
                }
            }
        }

        public IDictionary<string, string> GetValues()
        {
            var newValues = new Dictionary<string, string>();

            var propertyNames = _registry.GetManagedPropertyNames();
            foreach (var propertyName in propertyNames)
            {
                newValues.Add(propertyName, _registry.GetConfiguratorByPropertyName(propertyName).Value);
            }

            return newValues;
        }

        public bool IsFormValid()
        {
            var propertyNames = _registry.GetManagedPropertyNames();
            foreach (var propertyName in propertyNames)
            {
                var configurator = _registry.GetConfiguratorByPropertyName(propertyName);
                if (!configurator.IsValid)
                    return false;
            }
            return true;
        }
    }


    public class FormEventDispatcher
    {
        public event EventHandler EditingCancelled;
        public event EventHandler<CommittedChangesArgs> ChangesCommitted;


        public void RaiseEditingSessionCancelled()
        {
            EditingCancelled?.Invoke(this,new EventArgs());
        }


        public bool RaiseCommitChanges(IDictionary<string,string> newValues)
        {
            var committedChanges = new CommittedChangesArgs{NewValues = newValues};
            ChangesCommitted?.Invoke(this,committedChanges);
            return true;
        }
    }

    public class PagePreviewEventsDispatcher
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
