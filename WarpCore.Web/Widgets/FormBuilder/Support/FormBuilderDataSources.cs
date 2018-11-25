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
        IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model);
    }

    public class ListSourceDependency
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }


    public class CompositeConfiguratorTypeAttribute : Attribute
    {
    }

    public interface IUserInterfaceBehavior
    {
        void RegisterBehavior(IConfiguratorControl control, ConfiguratorBuildArguments buildArguments);
    }

    public interface ILabeledConfiguratorControl : IConfiguratorControl
    {
        string DisplayName { get; }
    }

    public class ConfiguratorBehaviorCollection : List<string>, ISupportsJavaScriptSerializer
    {
    }

    public interface IConfiguratorControl
    {
        void InitializeEditingContext(ConfiguratorBuildArguments buildArguments);
        string PropertyName { get; }
        void SetValue(string newValue);
        string GetValue();
        void SetConfiguration(SettingProperty settingProperty);
        ConfiguratorBehaviorCollection Behaviors { get; }
    }

    public class ContentControlSourceAttribute : Attribute, IListControlSource
    {
        private readonly string _repositoryUid;

        public ContentControlSourceAttribute(string repositoryUid)
        {
            _repositoryUid = repositoryUid;
        }

        public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string, string> model)
        {
            var repositoryUid = model.Get<Guid?>(_repositoryUid);
            if (repositoryUid == null)
                yield break;


            var t = RepositoryTypeResolver.ResolveTypeByApiId(new Guid(_repositoryUid));
            var repo = (IContentRepository)Activator.CreateInstance(t);

            IReadOnlyCollection<WarpCoreEntity> foundEntities;
            var versioned = repo as IVersionedContentRepositoryBase;
            var unversioned = repo as IUnversionedContentRepositoryBase;
            if (versioned != null)
            {
                foundEntities =versioned.FindContentVersions(null, ContentEnvironment.Draft);
            }
            else
            {
                foundEntities =unversioned.FindContent(null);
            }

            foreach (var foundEntity in foundEntities)
                yield return new ListOption
                {
                    Text = foundEntity.GetTitle(),
                    Value = foundEntity.ContentId.ToString()
                };
        }
    }

    public class EntitiesControlSourceAttribute : Attribute, IListControlSource
    {
        private readonly string _repositoryUidProperty;

        public EntitiesControlSourceAttribute(string repositoryUidProperty)
        {
            _repositoryUidProperty = repositoryUidProperty;
        }

        public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
        {
            var repositoryUid = model.Get<Guid?>(_repositoryUidProperty);
            if (repositoryUid == null)
                yield break;

            var mgr = new RepositoryMetadataManager();
            var repoMetadata = mgr.GetRepositoryMetdataByTypeResolverUid(repositoryUid.Value);
            var repoType = RepositoryTypeResolver.ResolveTypeByApiId(repoMetadata.ApiId);
            var repo = (IContentRepository)Activator.CreateInstance(repoType);
            var entityType = repo.New().GetType();
            var apiAttr = entityType.GetCustomAttribute<WarpCoreEntityAttribute>();
                yield return new ListOption
               {
                   Text = entityType.GetDisplayName(),
                   Value = apiAttr?.TypeExtensionUid.ToString()
                };
        }
    }

    public class RepositoryListControlSourceAttribute : Attribute, IListControlSource
    {
        public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
        {
            var mgr = new RepositoryMetadataManager();
            foreach (var repo in mgr.Find())
            {
                var t=RepositoryTypeResolver.ResolveTypeByApiId(repo.ApiId);

                var displayName = repo.CustomRepositoryName ?? t.GetDisplayName();

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

        public static List<PropertyInfo> PropertiesFilered(ConfiguratorBuildArguments buildArguments)
        {
            var t = GetClrType(buildArguments.ParentEditingContext);
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
        public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
        {
            var propertiesFilered = ConfiguratorEditingContextHelper.PropertiesFilered(buildArguments);
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

        public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
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
    public class PropertiesAsOptionListSource : Attribute, IListControlSource
    {
        private readonly Type[] _propertyTypes;

        public PropertiesAsOptionListSource()
        {
            _propertyTypes = null;
        }


        public PropertiesAsOptionListSource(params Type[] propertyTypes)
        {
            _propertyTypes = propertyTypes;
        }

        public IEnumerable<ListOption> GetOptions(ConfiguratorBuildArguments buildArguments, IDictionary<string,string> model)
        {
            var propertiesFilered = ConfiguratorEditingContextHelper.PropertiesFilered(buildArguments);

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