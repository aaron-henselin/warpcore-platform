using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using Cms_PageDesigner_Context;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Kernel;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace DemoSite
{


    public class ConfiguratorControlState
    {
        public Guid PageContentId { get; set; }
        public bool IsOpening { get; set; }
        
    }

    public partial class Configurator1 : System.Web.UI.UserControl
    {
        private CmsPageContent _contentToEdit;
        private ConfiguratorControlState _configuratorControlState;
        private List<IConfiguratorControl> _activatedConfigurators;


        public string WC_CONFIGURATOR_CONTEXT_JSON { get; set; }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SaveButton.Click += SaveButton_Click;

            var configuratorContextJson = Request["WC_CONFIGURATOR_CONTEXT_JSON"];
            if (string.IsNullOrWhiteSpace(configuratorContextJson))
                return;

            _configuratorControlState = new JavaScriptSerializer().Deserialize<ConfiguratorControlState>(configuratorContextJson);
            var editingContextManager = new EditingContextManager();
            try
            {
                var editingContext = editingContextManager.GetEditingContext();
                _contentToEdit = editingContext.FindSubContentReursive(x => x.Id == _configuratorControlState.PageContentId).Single().LocatedContent;
            }
            catch (Exception)
            {
                return;
            }
          
            
            
            RebuildDesignSurface();
        }


        private void SaveButton_Click(object sender, EventArgs e)
        {
            var newParameters = CmsFormReadWriter.ReadValuesFromControls(_activatedConfigurators);
            _contentToEdit.Parameters = newParameters.ToDictionary(x => x.Key,x => x.Value);
            ScriptManager.RegisterClientScriptBlock(this.Page,typeof(Configurator1),"submit", "configurator_submit();",true);
        }

        private void RebuildDesignSurface()
        {
            //ConfiguratorFormBuilderRuntimePlaceHolder.DataBind();
            //ConfiguratorFormBuilderRuntimePlaceHolder.Controls.Clear();

            //var toolboxItem = new ToolboxManager().GetToolboxItemByCode(_contentToEdit.WidgetTypeCode);
            
            //var activatedControl = new CmsPageContentActivator().ActivateCmsPageContent(_contentToEdit.ToPresentationElement());
            //var parametersAfterActivation = activatedControl.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);

            //var cmsForm=ConfiguratorFormBuilder.GenerateDefaultFormForWidget(toolboxItem);
            //_activatedConfigurators = new List<IConfiguratorControl>();// =CmsPageLayoutEngine.ActivateAndPlaceContent(surface, cmsForm.DesignedContent).OfType<IConfiguratorControl>().ToList();

            //var buildArguments = new ConfiguratorBuildArguments
            //{
            //    PageContentId = _configuratorControlState.PageContentId,
            //    ClrType = activatedControl.GetType(),
            //    PropertyFilter = ToolboxPropertyFilter.SupportsDesigner,
            //    DefaultValues = parametersAfterActivation,
            //    ParentEditingContext = new EditingContextManager().GetEditingContext(),
            //};
            //buildArguments.Events = CmsFormReadWriter.AddEventTracking(surface, buildArguments,_activatedConfigurators).Events;

            //CmsFormReadWriter.InitializeEditing(_activatedConfigurators, buildArguments);
            //CmsFormReadWriter.SetDefaultValues(_activatedConfigurators, buildArguments);

        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            //if (_configuratorControlState != null)
            //{
            //    var eventTracking = CmsFormReadWriter.GetEventTracking(surface);
            //    eventTracking.RaiseEvents();
            //    eventTracking.UpdateEventData();
            //}


        }





        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (_configuratorControlState != null)
            {
                _configuratorControlState.IsOpening = false;
                
                WC_CONFIGURATOR_CONTEXT_JSON =
                    Server.HtmlEncode(new JavaScriptSerializer().Serialize(_configuratorControlState));

                DataBoundElements.DataBind();
            }

        }

        protected void ConfiguratorInitButton_OnClick(object sender, EventArgs e)
        {
            RebuildDesignSurface();

        }
    }
}