using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using Cms;
using Cms.Forms;
using Cms.Toolbox;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Web.Widgets.FormBuilder.Configurators;

namespace WarpCore.Web.Widgets.FormBuilder.Support
{
    public class ConfiguratorFormBuilder
    {
        public const string RuntimePlaceHolderId = "FormBody";

        private static Guid ToGuid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static CmsForm GenerateDefaultFormForWidget(ToolboxItem toolboxItem)
        {
            var clrType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);

            return GenerateDefaultForm(clrType);
        }

        public static CmsForm GenerateDefaultForm(Type clrType)
        {
            var cmsForm = new CmsForm();

            var factory = new CmsPageContentBuilder();
            var rowLayout = factory.BuildCmsPageContentFromWebFormsControl(new RowLayout2 { NumColumns = 1 });
            rowLayout.Id = ToGuid(1);
            rowLayout.PlacementContentPlaceHolderId = RuntimePlaceHolderId;
            cmsForm.FormContent.Add(rowLayout);

            var configuratorSettingProperties =
                ToolboxMetadataReader.ReadProperties(clrType, ToolboxPropertyFilter.SupportsDesigner);

            foreach (var property in configuratorSettingProperties)
            {
                CmsPageContent content = null;

                if (false)
                {
                   
                }
                else
                {
                    if (property.ConfiguratorType != null)
                    {
                        content = CreateConfiguratorPageContent(property.ConfiguratorType, property);
                    }
                    else
                    {
                        var bestGuess = GetBestGuessForSettingType(property);
                        switch (bestGuess)
                        {
                            case Editor.SubForm:
                                content = CreateConfiguratorPageContent<CompositeConfigurator>(property);
                                break;

                            case Editor.RichText:
                            case Editor.Text:
                                content = CreateConfiguratorPageContent<ConfiguratorTextBox>(property);
                                break;

                            case Editor.OptionList:
                                content = CreateConfiguratorPageContent<ConfiguratorDropDownList>(property);
                                break;

                            case Editor.CheckBox:
                                content = CreateConfiguratorPageContent<ConfiguratorCheckBox>(property);
                                break;

                            case Editor.Hidden:
                                content = CreateConfiguratorPageContent<ConfiguratorHiddenField>(property);
                                break;

                            case Editor.Url:
                                content = CreateConfiguratorPageContent<ConfiguratorUrlSelector>(property);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                }
                

                content.PlacementLayoutBuilderId = rowLayout.Id;
                rowLayout.AllContent.Add(content);
            }

            return cmsForm;
        }

        private static CmsPageContent CreateConfiguratorPageContent(Type type, SettingProperty property)
        {
            IConfiguratorControl dropdownList = (IConfiguratorControl)Activator.CreateInstance(type);
            dropdownList.SetConfiguration(property);
            
            CmsPageContentBuilder builder = new CmsPageContentBuilder();
            var createdPageContent = builder.BuildCmsPageContentFromWebFormsControl((Control)dropdownList);
            return createdPageContent;
        }

        private static CmsPageContent CreateConfiguratorPageContent<TConfiguratorType>(SettingProperty property) where TConfiguratorType : IConfiguratorControl
        {
            return CreateConfiguratorPageContent(typeof(TConfiguratorType), property);
        }

        private static Editor GetBestGuessForSettingType(SettingProperty property)
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
    }
}