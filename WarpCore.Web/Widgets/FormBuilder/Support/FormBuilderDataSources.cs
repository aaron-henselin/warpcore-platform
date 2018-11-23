using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Cms.Toolbox;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Platform.Orm;

namespace WarpCore.Web.Widgets.FormBuilder.Support
{
    public interface IListControlSource
    {
        IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext);
    }

    public class CompositeConfiguratorTypeAttribute :Attribute
    {
    }

    public interface IUserInterfaceBehavior
    {
        void RegisterBehavior(IConfiguratorControl control, ConfiguratorEditingContext editingContext);
    }

    public interface ILabeledConfiguratorControl : IConfiguratorControl
    {
        string DisplayName { get; }
    }

    public class ConfiguratorBehaviorCollection : List<string>, ISupportsJsonTypeConverter
    {
    }

    public interface IConfiguratorControl
    {
        void InitializeEditingContext(ConfiguratorEditingContext editingContext);
        string PropertyName { get; }
        void SetValue(string newValue);
        string GetValue();
        void SetConfiguration(SettingProperty settingProperty);
        ConfiguratorBehaviorCollection Behaviors { get; }
    }


    public class RepositoryListControlSourceAttribute : Attribute, IListControlSource
    {
        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {
            

            var mgr = new RepositoryMetadataManager();
            foreach (var repo in mgr.Find())
            {
                var t=RepositoryTypeResolver.ResolveTypeByApiId(repo.ApiId);
                
                var displayName = repo.CustomRepositoryName 
                                    ?? t.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName 
                                    ?? t.Name;

                yield return new ListOption
                {
                    Text = displayName,
                    Value = repo.ApiId.ToString()
                };
            }
        }
    }

    public class ConfiguratorEditingContextHelper
    {
        public static Type GetClrType(EditingContext editingContext)
        {
            var repoManager = new RepositoryMetadataManager();
            var repoMetadata =
                repoManager.GetRepositoryMetdataByTypeResolverUid(editingContext.DesignContentTypeId);
            var repoType = Type.GetType(repoMetadata.AssemblyQualifiedTypeName);
            var repo = (IContentRepository)Activator.CreateInstance(repoType);
            return repo.New().GetType();
            
        }

        public static List<PropertyInfo> PropertiesFilered(ConfiguratorEditingContext editingContext)
        {
            var t = GetClrType(editingContext.ParentEditingContext);
            var propertiesFilered =
                t.GetPropertiesFiltered(ToolboxPropertyFilter.SupportsDesigner)
                    .ToList();
            return propertiesFilered;
        }
    }

    public static class PropertyDataSourceHelper
    {
        public static string CreateListOptionLabel(SettingProperty metadata)
        {
            var hasCustomDisplayName = !string.Equals(metadata.DisplayName, metadata.PropertyInfo.Name);

            string label = metadata.PropertyInfo.Name;
            if (hasCustomDisplayName)
                label = $"{metadata.DisplayName} ({metadata.PropertyInfo.Name})";
            return label;
        }
    }

    public class CompositeOnlyPropertiesDataSourceAttribute : Attribute, IListControlSource
    {
        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {
            var propertiesFilered = ConfiguratorEditingContextHelper.PropertiesFilered(editingContext);
            var props = propertiesFilered.Where(x =>
                x.PropertyType.GetCustomAttributes<CompositeConfiguratorTypeAttribute>().Any());

            foreach (var prop in props)
            {
                var metadata = ToolboxMetadataReader.GetPropertyMetadata(prop);
                var label = PropertyDataSourceHelper.CreateListOptionLabel(metadata);

                yield return new ListOption
                {
                    Text=label,
                    Value = prop.Name
                };
            }
        }


    }


    public class FixedOptionListDataSourceAttribute : Attribute, IListControlSource
    {
        private readonly string[] _options;

        public FixedOptionListDataSourceAttribute(params string[] options)
        {
            _options = options;
        }

        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {
            foreach (var option in _options)
            {

                yield return new ListOption
                {
                    Text = option,
                    Value = option
                };
            }
        }
    }
    public class FormControlPropertiesDataSourceAttribute : Attribute, IListControlSource
    {
        private readonly Type[] _propertyTypes;

        public FormControlPropertiesDataSourceAttribute()
        {
            _propertyTypes = null;
        }


        public FormControlPropertiesDataSourceAttribute(params Type[] propertyTypes)
        {
            _propertyTypes = propertyTypes;
        }

        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {
            var propertiesFilered = ConfiguratorEditingContextHelper.PropertiesFilered(editingContext);

            if (_propertyTypes != null)
                propertiesFilered=propertiesFilered.Where(x => _propertyTypes.Contains(x.PropertyType)).ToList();

            foreach (var prop in propertiesFilered)
            {
                var metadata = ToolboxMetadataReader.GetPropertyMetadata(prop);
                var label = PropertyDataSourceHelper.CreateListOptionLabel(metadata);

                yield return new ListOption
                {
                    Text = label,
                    Value = prop.Name
                };
            }
        }


    }

}