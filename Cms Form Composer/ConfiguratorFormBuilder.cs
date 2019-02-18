using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlazorComponents.Shared;
using Cms.Forms;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.UserInteraceHints;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

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

            if (editor == Editor.SubForm)
                return SubFormToolboxItem.ApiId;

            if (editor == Editor.Static)
                return StaticContentToolboxItem.ApiId;

            return TextboxToolboxItem.ApiId;
        }

        public CmsPageContent CreateCmsPageContent(SettingProperty property,string widgetTypeCode, FormStyle parentFormStyle)
        {
            var setup = new ConfiguratorSetup
            {
                PropertyType = property.PropertyInfo.PropertyType.FullName,
                DisplayName = property.DisplayName,
                PropertyName = property.PropertyInfo.Name
            };

            var cmsPageContent = new CmsPageContent
            {
                Id = Guid.NewGuid(),
                WidgetTypeCode = widgetTypeCode
            };

            var standardParameters = setup.GetPropertyValues(x => true).ToDictionary(x => x.Key, x => x.Value);
            var datasourceParameters = new Dictionary<string,string>();

            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(widgetTypeCode);

            if (toolboxItem.RequiresDataSource)
            {
                var dataRelationDs = InferDataSourceFromDataRelation(property);
                if (dataRelationDs != null)
                    datasourceParameters = dataRelationDs.GetPropertyValues(x => true).ToDictionary(x => x.Key, x => x.Value);

            }

            if (toolboxItem.SupportsSubContent)
            {
                var propType = property.PropertyInfo.PropertyType;
                var defaultForm = GenerateDefaultForm(propType,parentFormStyle);
                cmsPageContent.AllContent = defaultForm.ChildNodes;
            }
            

            var mergedParameters = new Dictionary<string,string>();
            foreach (var parameter in standardParameters)
                mergedParameters.Add(parameter.Key,parameter.Value);
            foreach (var parameter in datasourceParameters)
                mergedParameters.Add(parameter.Key, parameter.Value);
            cmsPageContent.Parameters = mergedParameters;

            return cmsPageContent;
        }



        private static RequiresDataSource InferDataSourceFromDataRelation(SettingProperty property)
        {
            var fixedOptions = property.PropertyInfo.GetCustomAttribute<FixedOptionsDataSourceAttribute>();
            if (fixedOptions != null)
            {
                var collection = new DataSourceItemCollection();
                var allItems = fixedOptions.Options.Select(x => new DataSourceItem(x)).ToList();
                collection.Items.AddRange(allItems);
                return new RequiresDataSource
                {
                    DataSourceType = DataSourceTypes.FixedItems,
                    Items = collection
                };
            }

            var dataRelation = property.PropertyInfo.GetCustomAttribute<DataRelationAttribute>();
            if (dataRelation != null)
                return new RequiresDataSource
                {
                    DataSourceType = DataSourceTypes.Repository,
                    RepositoryApiId = new Guid(dataRelation.ApiId)
                };

            return null;
        }

        private class RequiresDataSource : IRequiresDataSource
        {
            public Guid RepositoryApiId { get; set; }
            public DataSourceItemCollection Items { get; set; }
            public string DataSourceType { get; set; }
        }

        private class ConfiguratorSetup : BlazorToolboxItem
        {
            //public string PropertyName { get; set; }
            //public string DisplayName { get; set; }
            //public string PropertyType { get; set; }
            //public string WidgetTypeCode { get; set; }
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

        public CmsContentListDefinition GenerateDefaultContentListDefinition(Type clrType)
        {
            var configuratorSettingProperties =
                ToolboxMetadataReader.ReadProperties(clrType, ToolboxPropertyFilter.SupportsDesigner);

            var definition = new CmsContentListDefinition();
            foreach (var property in configuratorSettingProperties)
            {
                var ignore = 
               property.PropertyInfo.PropertyType == typeof(Guid?)||
               property.PropertyInfo.PropertyType == typeof(Guid)||
               property.PropertyInfo.GetCustomAttribute<SerializedComplexObjectAttribute>() != null;

                if (!ignore)
                definition.Fields.Add(new CmsListField
               {
                   Id = Guid.NewGuid(),
                   Label = property.DisplayName,
                   PropertyName = property.PropertyInfo.Name
               });
            }

            return definition;
        }

        public  CmsForm GenerateDefaultForm(Type clrType, FormStyle formStyle)
        {
            if (formStyle == FormStyle.Undefined)
                throw new ArgumentException(nameof(formStyle));


            var builder = new ConfiguratorCmsPageContentBuilder();

            var cmsForm = new CmsForm();
            var row = builder.CreateRow(1);
            cmsForm.ChildNodes.Add(row);

            var configuratorSettingProperties =
                ToolboxMetadataReader.ReadProperties(clrType, ToolboxPropertyFilter.SupportsDesigner);

            foreach (var property in configuratorSettingProperties)
            {
                string widgetTypeCode = null;

                if (formStyle == FormStyle.Edit)
                {
                    if (property.WidgetTypeCode != null)
                    {
                        widgetTypeCode = property.WidgetTypeCode;
                    }
                    else
                    {
                        var editor = GetEditorForSettingProperty(property);
                        widgetTypeCode = AutoSelectWidgetForEditorCode(editor);
                    }
                }

                if (formStyle == FormStyle.Readonly)
                {
                    widgetTypeCode = AutoSelectWidgetForEditorCode(Editor.Static);
                }

                var content = builder.CreateCmsPageContent(property, widgetTypeCode,formStyle);
                content.PlacementContentPlaceHolderId = 0.ToString();
                content.Id = Guid.NewGuid();
                row.AllContent.Add(content);
            }

            return cmsForm;
        }


    }

    public enum FormStyle
    {
        Undefined,Edit, Readonly
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