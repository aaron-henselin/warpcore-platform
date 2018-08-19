using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using Cms.Toolbox;
using Framework;
using WarpCore.Cms.Toolbox;

namespace WarpCore.Web.Widgets.FormBuilder
{
    public interface IListControlSource
    {
        IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext);
    }

    public interface IConfiguratorControl
    {
        void InitializeEditingContext(ConfiguratorEditingContext editingContext);
    }

    public class RepositoryListControlSourceAttribute : Attribute, IListControlSource
    {
        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {
            

            var mgr = new RepositoryMetadataManager();
            foreach (var repo in mgr.Find())
            {
                var t = Type.GetType(repo.AssemblyQualifiedTypeName);
                var displayName = t.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? t.Name;
                yield return new ListOption
                {
                    Text = displayName,
                    Value = repo.FormInteropUid
                };
            }
        }
    }

    public class PropertyListControlSourceAttribute : Attribute, IListControlSource
    {
        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {
            var propertiesFilered = editingContext.ClrType.GetPropertiesFiltered(editingContext.PropertyFilter).ToList();
            var propertyNames = propertiesFilered.Select(x => x.Name);
            return propertyNames.Select(x => new ListOption { Text = x, Value = x });
        }
    }

}