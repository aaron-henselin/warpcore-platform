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
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public class ConfiguratorTextBox : PlaceHolder
    {
        private TextBox _tbx = new TextBox { AutoPostBack = true,CssClass = "form-control"};
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }

        public string Value
        {
            get { return _tbx.Text; }
            set { _tbx.Text = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = PropertyName;

            this.Controls.Add(new Label { Text = DisplayName });
            _tbx.ID = PropertyName + "_TextBox";
            this.Controls.Add(_tbx);
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
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
            {
                newParameters.Add(tbx.PropertyName, tbx.Value);
            }


            _contentToEdit.Parameters = newParameters;
            ScriptManager.RegisterClientScriptBlock(this.Page,typeof(Configurator1),"submit", "configurator_submit();",true);
            //

        }

        private void RebuildDesignSurface()
        {
            surface.Controls.Clear();
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(_contentToEdit.WidgetTypeCode);
            var _toolboxItemType = CmsPageContentActivator.ResolveToolboxItemType(toolboxItem);

            var activatedControl = CmsPageContentActivator.ActivateControl(toolboxItem,_contentToEdit.Parameters);
            var parametersAfterActivation = CmsPageContentActivator.GetContentParameterValues(activatedControl);




            foreach (var property in _toolboxItemType.GetProperties())
            {


                var displayNameDefinition = (DisplayNameAttribute) property.GetCustomAttribute(typeof(DisplayNameAttribute));
                var settingDefinition = (SettingAttribute) property.GetCustomAttribute(typeof(SettingAttribute));
                if (settingDefinition != null)
                {
                    var wrapper = new HtmlGenericControl("div");
                    wrapper.Attributes["class"] = "form-group";

                    var textbox = new ConfiguratorTextBox
                    {
                        PropertyName = property.Name,
                        DisplayName = property.Name,
                    };

                    if (displayNameDefinition != null)
                        textbox.DisplayName = displayNameDefinition.DisplayName;

                   
                        textbox.Value = parametersAfterActivation[property.Name];

                    wrapper.Controls.Add(textbox);
                    surface.Controls.Add(wrapper);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //_contentToEdit.Parameters.Clear();



            if (_configuratorContext != null)
            {

                //_configuratorContext.NewConfiguration = newParameters;
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