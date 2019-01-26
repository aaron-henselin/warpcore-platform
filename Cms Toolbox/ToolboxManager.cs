using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Orm;

namespace WarpCore.Cms.Toolbox
{
    [Table("cms_toolbox_client_application")]
    public class BlazorApplication : UnversionedContentEntity
    {
     
        public List<BlazorAssemblyReference> AssemblyReferences { get; set; } = new List<BlazorAssemblyReference>();
    }

    public class BlazorAssemblyReference
    {
    }

    [Table("cms_toolbox_item")]
    public class ToolboxItem : UnversionedContentEntity
    {
        public string WidgetUid { get; set; }
        public string Description { get; set; }
        public string PresentationEngine { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string AscxPath { get; set; }
        public string Category { get; set; }
        public string FriendlyName { get; set; }
    }

    public static class ToolboxBootstrapper
    {
        public static void RegisterToolboxItemsWithApi(AppDomain appDomain)
        {
            var assemblies = appDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();

            var toIncludeInToolbox = allTypes
                .Select(ToolboxMetadataReader.ReadMetadata)
                .Where(x => x != null);

            var mgr = new ToolboxManager();
            var alreadyInToolbox = mgr.Find().ToDictionary(x => x.WidgetUid);
            foreach (var discoveredToolboxItem in toIncludeInToolbox)
            {
                //var includeInToolboxAtr = typeToInclude.GetCustomAttribute<IncludeInToolboxAttribute>();

                ToolboxItem widget;
                if (alreadyInToolbox.ContainsKey(discoveredToolboxItem.WidgetUid))
                    widget = alreadyInToolbox[discoveredToolboxItem.WidgetUid];
                else
                    widget = new ToolboxItem();

                widget.WidgetUid = discoveredToolboxItem.WidgetUid;
                widget.FriendlyName = discoveredToolboxItem.FriendlyName;
                widget.AssemblyQualifiedTypeName = discoveredToolboxItem.AssemblyQualifiedTypeName;
                widget.Category = discoveredToolboxItem.Category;
                widget.AscxPath = discoveredToolboxItem.AscxPath;
                mgr.Save(widget);

                alreadyInToolbox = mgr.Find().ToDictionary(x => x.WidgetUid);
            }

        }
    }

    public class ToolboxManager : UnversionedContentRepository<ToolboxItem>
    {
        public ToolboxItem GetToolboxItemByCode(string code)
        {
            var toolboxResult = Orm.FindUnversionedContent<ToolboxItem>("WidgetUid eq '" + code + "'").Result;
            if (!toolboxResult.Any())
                throw new Exception($"Toolbox does not contain item '{code}'");

            return toolboxResult.Single();
        }

        //public static Type ResolveToolboxItemClrType(ToolboxItem toolboxItem)
        //{
        //    if (!string.IsNullOrWhiteSpace(toolboxItem.AscxPath))
        //    {
        //        return BuildManager.GetCompiledType(toolboxItem.AscxPath);
        //    }

        //    return Type.GetType(toolboxItem.AssemblyQualifiedTypeName);
        //}
    }
}
