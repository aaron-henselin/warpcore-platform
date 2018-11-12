using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Cms;
using Cms.Forms;
using Cms.Toolbox;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;
using WarpCore.Web;
using WarpCore.Web.Widgets;
using WarpCore.Web.Widgets.FormBuilder;

namespace DemoSite
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
            var cmsForm = new CmsForm();

            var factory = new CmsPageContentFactory();
            var rowLayout = factory.CreateToolboxItemContent(new RowLayout { NumColumns = 1});
            rowLayout.Id = ToGuid(1);
            rowLayout.PlacementContentPlaceHolderId = RuntimePlaceHolderId;
            cmsForm.FormContent.Add(rowLayout);
           
            var clrType = ToolboxManager.ResolveToolboxItemClrType(toolboxItem);
            var defaults = CmsPageContentActivator.GetDefaultContentParameterValues(toolboxItem);
            var configuratorSettingProperties = ToolboxMetadataReader.ReadProperties(clrType,ToolboxPropertyFilter.IsConfigurable);

            foreach (var property in configuratorSettingProperties)
            {
                if (property.SettingType == null || property.SettingType == SettingType.Text)
                {
                    var textboxPageContent =
                        factory.CreateToolboxItemContent(new ConfiguratorTextBox
                        {
                            PropertyName = property.PropertyInfo.Name,
                            DisplayName = property.DisplayName
                        });

                    textboxPageContent.PlacementLayoutBuilderId = rowLayout.Id;
                    rowLayout.AllContent.Add(textboxPageContent);
                }
                else
                {
                    var textboxPageContent =
                        factory.CreateToolboxItemContent(new ConfiguratorDropDownList
                        {
                            PropertyName = property.PropertyInfo.Name,
                            DisplayName = property.DisplayName,
                                                        //RepositoryUid = defaults[property.PropertyInfo.Name] //todo: no idea how to handle this one yet.
                        });

                    textboxPageContent.PlacementLayoutBuilderId = rowLayout.Id;
                    rowLayout.AllContent.Add(textboxPageContent);
                }
            }

            return cmsForm;
        }
    }

    public class ConfiguratorContext
    {
        public Guid PageContentId { get; set; }
        public bool IsOpening { get; set; }

        public Dictionary<string,string> NewConfiguration { get; set; }
    }

    public partial class Configurator1 : System.Web.UI.UserControl
    {
        private CmsPageContent _contentToEdit;
        private ConfiguratorContext _configuratorContext;
        public string WC_CONFIGURATOR_CONTEXT_JSON { get; set; }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SaveButton.Click += SaveButton_Click;

            var configuratorContextJson = Request["WC_CONFIGURATOR_CONTEXT_JSON"];
            if (string.IsNullOrWhiteSpace(configuratorContextJson))
                return;

            _configuratorContext = new JavaScriptSerializer().Deserialize<ConfiguratorContext>(configuratorContextJson);
            var editingContextManager = new EditingContextManager();
            try
            {
                var editingContext = editingContextManager.GetEditingContext();
                _contentToEdit = editingContext.FindSubContentReursive(x => x.Id == _configuratorContext.PageContentId).Single().LocatedContent;
            }
            catch (Exception)
            {
                return;
            }
          
            

            //WC_EDITING_CONTEXT_JSON
            RebuildDesignSurface();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var newParameters = CmsFormReadWriter.ReadValuesFromControls(surface);
            _contentToEdit.Parameters = newParameters.ToDictionary(x => x.Key,x => x.Value);
            ScriptManager.RegisterClientScriptBlock(this.Page,typeof(Configurator1),"submit", "configurator_submit();",true);
        }

        private void RebuildDesignSurface()
        {
            ConfiguratorFormBuilderRuntimePlaceHolder.DataBind();
            ConfiguratorFormBuilderRuntimePlaceHolder.Controls.Clear();

            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(_contentToEdit.WidgetTypeCode);

            var activatedControl = CmsPageContentActivator.ActivateControl(toolboxItem,_contentToEdit.Parameters);
            var parametersAfterActivation = activatedControl.GetPropertyValues(ToolboxPropertyFilter.IsConfigurable);

            var cmsForm=ConfiguratorFormBuilder.GenerateDefaultFormForWidget(toolboxItem);
            CmsPageLayoutEngine.ActivateAndPlaceContent(surface, cmsForm.DesignedContent);

            var configuratorEditingContext = new ConfiguratorEditingContext
            {
                ClrType = activatedControl.GetType(),
                PropertyFilter = ToolboxPropertyFilter.IsConfigurable,
                CurrentValues = parametersAfterActivation,
                ParentEditingContext = new EditingContextManager().GetEditingContext()
            };
            
            CmsFormReadWriter.PopulateListControls(surface, configuratorEditingContext);
            CmsFormReadWriter.FillInControlValues(surface, configuratorEditingContext);


        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (_configuratorContext != null)
            {
                _configuratorContext.IsOpening = false;
                
                WC_CONFIGURATOR_CONTEXT_JSON =
                    Server.HtmlEncode(new JavaScriptSerializer().Serialize(_configuratorContext));

                DataBoundElements.DataBind();
            }

        }

        protected void ConfiguratorInitButton_OnClick(object sender, EventArgs e)
        {
            RebuildDesignSurface();
        }
    }
}