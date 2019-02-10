using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorComponents.Shared;
using Microsoft.AspNetCore.Blazor.Components;

namespace BlazorComponents.Client
{



    public class ConfiguratorRegistry
    {
        public Dictionary<string, IConfiguratorComponent> RegisteredComponents { get; set; } = new Dictionary<string, IConfiguratorComponent>();

        public void RegisterComponent(string propertyName, IConfiguratorComponent blazorComponent)
        {
            Console.WriteLine($@"[Forms] Registering blazor form control for {propertyName}");

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

        public void SetDataSources(IDictionary<string, LocalDataSource> localDataSources)
        {
            var propertyNames = _registry.GetManagedPropertyNames();
            foreach (var propertyName in propertyNames)
            {
                if (localDataSources.ContainsKey(propertyName))
                {
                    var hasLocalDataSource = (_registry.GetConfiguratorByPropertyName(propertyName) as IHasLocalDataSource);
                    if (hasLocalDataSource != null)
                        hasLocalDataSource.LocalDataSource = localDataSources[propertyName];
                }
            }
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

    public class MutableKeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }


}
