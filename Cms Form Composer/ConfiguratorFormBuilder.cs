using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlazorComponents.Shared;
using Cms.Forms;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Kernel;

namespace WarpCore.Web.Widgets.FormBuilder.Support
{
    //public class ConfiguratorWidgetTypes
    //{
    //    public const string SubForm = "wc-configurator-subform";
    //    public const string TextBox = "wc-configurator-textbox";
    //    public const string OptionList = "wc-configurator-optionlist";
    //    public const string CheckBox = "wc-configurator-checkbox";
    //    public const string HiddenField = "wc-configurator-hiddenfield";
    //    public const string UrlSelector = "wc-configurator-urlselector";
    //}

    //public class ConfiguratorWidgetTypeSelector : IConfiguratorWidgetTypeSelector
    //{
    //    public string GetWidgetTypeCode(Editor editor)
    //    {
        
    //        switch (editor)
    //        {
    //            case Editor.SubForm:
    //                return ConfiguratorWidgetTypes.SubForm;
    //                break;

    //            case Editor.RichText:
    //            case Editor.Text:
    //                return ConfiguratorWidgetTypes.SubForm;
    //                break;

    //            case Editor.OptionList:
    //                return prefix + "OptionList";
    //                break;

    //            case Editor.CheckBox:
    //                return prefix + "CheckBox";
    //                break;

    //            case Editor.Hidden:
    //                return prefix + "HiddenField";
    //                break;

    //            case Editor.Url:
    //                return prefix + "UrlSelector";
    //                break;

    //            default:
    //                throw new ArgumentOutOfRangeException();
    //        }
    //    }
    //}


    public class ConfiguratorCmsPageContentBuilder
    {
        private static Editor GetEditorForSettingProperty(SettingProperty property)
        {
            Editor? bestGuess = property.Editor;
            if (bestGuess == null)
            {
                var requiresCompositedConfiguration =
                    property.PropertyInfo.PropertyType.GetCustomAttributes<CompositeConfiguratorTypeAttribute>().Any();
                if (requiresCompositedConfiguration)
                    return Editor.SubForm;

                var isBoolean = property.PropertyInfo.PropertyType == typeof(bool);
                if (isBoolean)
                    bestGuess = Editor.CheckBox;

                var isString = property.PropertyInfo.PropertyType == typeof(string);
                if (isString)
                    bestGuess = Editor.Text;

                var isInt = property.PropertyInfo.PropertyType == typeof(int) || property.PropertyInfo.PropertyType == typeof(int?);
                if (isInt)
                    bestGuess = Editor.Text;

                var isDecimal = property.PropertyInfo.PropertyType == typeof(decimal) || property.PropertyInfo.PropertyType == typeof(decimal?);
                if (isDecimal)
                    bestGuess = Editor.Text;

                var isUri = property.PropertyInfo.PropertyType == typeof(Uri);
                if (isUri)
                    bestGuess = Editor.Url;
                
                var hasDataRelation = property.PropertyInfo.GetCustomAttributes<DataRelationAttribute>().Any();
                if (hasDataRelation)
                    bestGuess = Editor.OptionList;

                var hasListControlSource = property.PropertyInfo.GetCustomAttributes().OfType<IListControlSource>().Any();
                if (hasListControlSource)
                    bestGuess = Editor.OptionList;



            }

            return bestGuess ?? Editor.SubForm;
        }

        private string AutoSelectWidgetForEditorCode(Editor editor)
        {
            if (editor == Editor.OptionList)
                return DropdownToolboxItem.ApiId;

            if (editor == Editor.CheckBox)
                return CheckboxToolboxItem.ApiId;

            if (editor == Editor.Url)
                return UriSelectorToolboxItem.ApiId;

            if (editor == Editor.RichText)
                return RichTextEditorToolboxItem.ApiId;

            if (editor == Editor.Text)
                return TextboxToolboxItem.ApiId;

            return TextboxToolboxItem.ApiId;
        }

        public CmsPageContent CreateCmsPageContent(SettingProperty property)
        {
            var setup = new ConfiguratorSetup
            {
                PropertyType = property.PropertyInfo.PropertyType.FullName,
                DisplayName = property.DisplayName,
                PropertyName = property.PropertyInfo.Name
            };
            
            
            if (property.WidgetTypeCode != null)
            {
                setup.WidgetTypeCode = property.WidgetTypeCode;
            }
            else
            {
                var editor = GetEditorForSettingProperty(property);
                setup.WidgetTypeCode = AutoSelectWidgetForEditorCode(editor);

            }

            var cmsPageContent = new CmsPageContent();
            cmsPageContent.Id = Guid.NewGuid();
            cmsPageContent.WidgetTypeCode = setup.WidgetTypeCode;
            cmsPageContent.Parameters = setup.GetPropertyValues(x => true).ToDictionary(x => x.Key, x => x.Value);

            return cmsPageContent;
        }

        private class ConfiguratorSetup
        {
            public string PropertyName { get; set; }
            public string DisplayName { get; set; }
            public string PropertyType { get; set; }
            public string WidgetTypeCode { get; set; }
        }

        public CmsPageContent CreateRow(int numColumns)
        {
           
            var row = new CmsPageContent
            {
                WidgetTypeCode = "ConfiguratorRow",
            };
            row.Id = Guid.NewGuid();
            row.Parameters[nameof(ConfiguratorRow.NumColumns)] = numColumns.ToString();
            return row;
        }


        public  CmsForm GenerateDefaultForm(Type clrType)
        {
            var builder = new ConfiguratorCmsPageContentBuilder();

            var cmsForm = new CmsForm();
            var row = builder.CreateRow(1);
            cmsForm.ChildNodes.Add(row);

            var configuratorSettingProperties =
                ToolboxMetadataReader.ReadProperties(clrType, ToolboxPropertyFilter.SupportsDesigner);

            foreach (var property in configuratorSettingProperties)
            {
                var content = builder.CreateCmsPageContent(property);
                content.PlacementContentPlaceHolderId = 0.ToString();
                content.Id = Guid.NewGuid();
                row.AllContent.Add(content);
            }

            return cmsForm;
        }


    }

    

    //public class ConfiguratorFormBuilder
    //{
    //    public ConfiguratorFormBuilder()
    //    {

    //    }

    //    public const string RuntimePlaceHolderId = "FormBody";



    //    public static CmsForm GenerateDefaultFormForWidget(ToolboxItem toolboxItem)
    //    {
    //        //todo: fix
    //        var clrType = Type.GetType(toolboxItem.AssemblyQualifiedTypeName);
    //        //var clrType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);

    //        return GenerateDefaultForm(clrType);
    //    }



    //    private static CmsPageContent CreateConfiguratorPageContent(Type type, SettingProperty property)
    //    {
    //        IConfiguratorControl dropdownList = (IConfiguratorControl)Activator.CreateInstance(type);
    //        dropdownList.SetConfiguration(property);
            
    //        CmsPageContentBuilder builder = new CmsPageContentBuilder();
    //        var createdPageContent = builder.BuildCmsPageContentFromWebFormsControl((Control)dropdownList);
    //        return createdPageContent;
    //    }

    //    private static CmsPageContent CreateConfiguratorPageContent<TConfiguratorType>(SettingProperty property) where TConfiguratorType : IConfiguratorControl
    //    {
    //        return CreateConfiguratorPageContent(typeof(TConfiguratorType), property);
    //    }


    //}

    //public class ConfiguratorBuildArguments
    //{
    //    public IDictionary<string, string> DefaultValues { get; set; }
    //    public Type ClrType { get; set; }
    //    public Func<PropertyInfo, bool> PropertyFilter { get; set; }
    //    public EditingContext ParentEditingContext { get; set; }

    //    public Guid PageContentId { get; set; }
    //    public ConfiguratorEvents Events { get; set; }
    //}
}