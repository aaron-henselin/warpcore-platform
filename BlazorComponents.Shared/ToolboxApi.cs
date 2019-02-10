using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using WarpCore.Platform.DataAnnotations;
[assembly: IsWarpCorePluginAssembly]
namespace BlazorComponents.Shared
{


    public class ToolboxViewModel
    {
        public List<ToolboxCategory> ToolboxCategories { get; set; } = new List<ToolboxCategory>();

    }

    public class ToolboxCategory
    {
        public bool IsClientSide { get; set; }
        public string CategoryName { get; set; }
        public List<ToolboxItemViewModel> Items { get; set; } = new List<ToolboxItemViewModel>();
    }

    public class ToolboxItemViewModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string WidgetTypeCode { get; set; }
    }

    public abstract class BlazorToolboxItem
    {
        [UserInterfaceHint]
        [DisplayName("Property Name")]
        public string PropertyName { get; set; }

        [UserInterfaceHint]
        [DisplayName("Display Name")]
        public string DisplayName { get; set; }

        [UserInterfaceHint]
        [DisplayName("Property Type")]
        public string PropertyType { get; set; }
    }



    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Textbox", Category = "Data Entry", UseClientSidePresentationEngine=true)]
    public class TextboxToolboxItem : BlazorToolboxItem
    {
        public const string ApiId = "warpcore-blazor-textbox";
    }

    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Dropdown", Category = "Data Entry", UseClientSidePresentationEngine = true)]
    public class DropdownToolboxItem : BlazorToolboxItem, IRequiresDataSource
    {
        public const string ApiId = "warpcore-blazor-dropdown";
        //todo: what to do with this??
        public const string RepositoryMetadataApiId = "3a9a6f79-9564-4b51-af1c-9d926fddbc35";
        [DataRelation(RepositoryMetadataApiId)]
        [UserInterfaceHint(Editor = Editor.OptionList)]
        [DisplayName("DataSource Repository")]
        public Guid RepositoryApiId { get; set; }

        [UserInterfaceHint(CustomEditorType = KeyValuePairsToolboxItem.ApiId)]
        [DisplayName("DataSource Items")]
        public DataSourceItemCollection Items { get; set; } = new DataSourceItemCollection();

        [UserInterfaceHint(Editor = Editor.OptionList)]
        [FixedOptionsDataSource(DataSourceTypes.Repository,DataSourceTypes.FixedItems)]
        public string DataSourceType { get; set; }
    }

    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Checkbox", Category = "Data Entry", UseClientSidePresentationEngine = true)]
    public class CheckboxToolboxItem : BlazorToolboxItem
    {
        public const string ApiId = "warpcore-blazor-checkbox";
    }

    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Rich Text Editor", Category = "Data Entry", UseClientSidePresentationEngine = true)]
    public class RichTextEditorToolboxItem : BlazorToolboxItem
    {
        public const string ApiId = "warpcore-blazor-richtexteditor";
    }


    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Uri Selector", Category = "Data Entry", UseClientSidePresentationEngine = true)]
    public class UriSelectorToolboxItem : BlazorToolboxItem
    {
        public const string ApiId = "warpcore-blazor-uriselector";
    }

    [WarpCore.Platform.DataAnnotations.ToolboxItem(WidgetUid = ApiId, FriendlyName = "Key Value Pairs", Category = "Data Entry", UseClientSidePresentationEngine = true)]
    public class KeyValuePairsToolboxItem : BlazorToolboxItem
    {
        public const string ApiId = "warpcore-blazor-keyvaluepairs";
    }
}
