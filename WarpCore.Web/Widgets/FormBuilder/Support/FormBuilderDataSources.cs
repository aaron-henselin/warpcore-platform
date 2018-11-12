using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using Cms.Toolbox;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Platform.Orm;

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
                var t=RepositoryTypeResolver.ResolveDynamicTypeByInteropId(new Guid(repo.ApiId));
                
                var displayName = repo.CustomRepositoryName 
                                    ?? t.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName 
                                    ?? t.Name;

                yield return new ListOption
                {
                    Text = displayName,
                    Value = repo.ApiId
                };
            }
        }
    }

    public class FormControlPropertiesDataSourceAttribute : Attribute, IListControlSource
    {
        public IEnumerable<ListOption> GetOptions(ConfiguratorEditingContext editingContext)
        {

            var repoManager = new RepositoryMetadataManager();
            var repoMetadata = repoManager.GetRepositoryMetdataByTypeResolverUid(editingContext.ParentEditingContext.DesignContentTypeId);
            var repoType = Type.GetType(repoMetadata.AssemblyQualifiedTypeName);
            var repo = (IContentRepository)Activator.CreateInstance(repoType);
            var t = repo.New().GetType();
            
            var propertiesFilered = 
                t.GetPropertiesFiltered(ToolboxPropertyFilter.IsNotIgnoredType)
                .ToList();

            var propertyNames = propertiesFilered.Select(x => x.Name);
            return propertyNames.Select(x => new ListOption { Text = x, Value = x });
        }
    }

}