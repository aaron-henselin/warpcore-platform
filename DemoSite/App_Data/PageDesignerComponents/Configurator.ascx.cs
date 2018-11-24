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
using WarpCore.Web.Widgets.FormBuilder.Configurators;
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
        private ConfiguratorEvents _events;

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
          
            

            _events = new ConfiguratorEvents();
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
            var parametersAfterActivation = activatedControl.GetPropertyValues(ToolboxPropertyFilter.SupportsDesigner);

            var cmsForm=ConfiguratorFormBuilder.GenerateDefaultFormForWidget(toolboxItem);
            CmsPageLayoutEngine.ActivateAndPlaceContent(surface, cmsForm.DesignedContent);

            var buildArguments = new ConfiguratorBuildArguments
            {
                PageContentId = _configuratorControlState.PageContentId,
                ClrType = activatedControl.GetType(),
                PropertyFilter = ToolboxPropertyFilter.SupportsDesigner,
                DefaultValues = parametersAfterActivation,
                ParentEditingContext = new EditingContextManager().GetEditingContext(),
                Events = _events
            };
           

            CmsFormReadWriter.InitializeEditing(surface, buildArguments);
            CmsFormReadWriter.FillInControlValues(surface, buildArguments);
            CmsFormReadWriter.AddEventTracking(surface, buildArguments);
            
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (_configuratorControlState != null)
            {
                var previousConfiguredContentId = CmsFormReadWriter.GetEventTracking(surface).PageContentId;
                var newConfiguredContentId = _configuratorControlState.PageContentId;
                var isSameForm = previousConfiguredContentId == newConfiguredContentId;
                if (isSameForm)
                {
                    var values = CmsFormReadWriter.GetChangedValues(surface);
                    foreach (var value in values)
                        _events.RaiseValueChanged(value);

                }


                var eventrcking= CmsFormReadWriter.GetEventTracking(surface);
                eventrcking.PreviousControlValues = CmsFormReadWriter.ReadValuesFromControls(surface);
                eventrcking.PageContentId = _configuratorControlState.PageContentId;


            }


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