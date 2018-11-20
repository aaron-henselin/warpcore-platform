using System;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using Cms;
using Cms.Forms;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
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

            var factory = new CmsPageContentFactory();
            var rowLayout = factory.CreateToolboxItemContent(new RowLayout { NumColumns = 1 });
            rowLayout.Id = ToGuid(1);
            rowLayout.PlacementContentPlaceHolderId = RuntimePlaceHolderId;
            cmsForm.FormContent.Add(rowLayout);

            var configuratorSettingProperties =
                ToolboxMetadataReader.ReadProperties(clrType, ToolboxPropertyFilter.SupportsDesigner);

            foreach (var property in configuratorSettingProperties)
            {
                CmsPageContent content = null;
                var requiresCompositedConfiguration =
                    property.PropertyInfo.PropertyType.GetCustomAttributes<CompositeConfiguratorTypeAttribute>().Any();
                if (requiresCompositedConfiguration)
                {
                    content = CreateConfiguratorPageContent<CompositeConfigurator>(property);
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
                            case SettingType.RichText:
                            case SettingType.Text:
                                content = CreateConfiguratorPageContent<ConfiguratorTextBox>(property);
                                break;

                            case SettingType.OptionList:
                                content = CreateConfiguratorPageContent<ConfiguratorDropDownList>(property);
                                break;

                            case SettingType.CheckBox:
                                content = CreateConfiguratorPageContent<ConfiguratorCheckBox>(property);
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

            CmsPageContentFactory factory = new CmsPageContentFactory();
            var createdPageContent = factory.CreateToolboxItemContent((Control)dropdownList);
            return createdPageContent;
        }

        private static CmsPageContent CreateConfiguratorPageContent<TConfiguratorType>(SettingProperty property) where TConfiguratorType : IConfiguratorControl
        {
            return CreateConfiguratorPageContent(typeof(TConfiguratorType), property);
        }

        private static SettingType GetBestGuessForSettingType(SettingProperty property)
        {
            SettingType? bestGuess = property.SettingType;
            if (bestGuess == null)
            {
                var isBoolean = property.PropertyInfo.PropertyType == typeof(bool);
                if (isBoolean)
                    bestGuess = SettingType.CheckBox;

                var isInt = property.PropertyInfo.PropertyType == typeof(int);
                if (isInt)
                    bestGuess = SettingType.Text;

                var isDecimal = property.PropertyInfo.PropertyType == typeof(decimal);
                if (isDecimal)
                    bestGuess = SettingType.Text;

                var hasDataRelation = property.PropertyInfo.GetCustomAttributes<DataRelationAttribute>().Any();
                if (hasDataRelation)
                    bestGuess = SettingType.OptionList;

                var hasListControlSource = property.PropertyInfo.GetCustomAttributes().OfType<IListControlSource>().Any();
                if (hasListControlSource)
                    bestGuess = SettingType.OptionList;

                

            }

            return bestGuess ?? SettingType.Text;
        }
    }
}