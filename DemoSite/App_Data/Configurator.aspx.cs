using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public class ConfiguratorTextBox :PlaceHolder
    {
        private TextBox _tbx = new TextBox{AutoPostBack = true};
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

    public partial class Configurator : System.Web.UI.Page
    {
        private EditingContext _ec;
        private Type _toolboxItemType;
        private CmsPageContent _existingContent;
        private Control _activatedControl;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Form.Action = HttpContext.Current.Request.RawUrl;
            
            var widgetType = Request["widgetType"];
            if (string.IsNullOrWhiteSpace(widgetType))
                return;

            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(widgetType);
            _toolboxItemType = CmsPageContentActivator.ResolveToolboxItemType(toolboxItem);

            var editingContextManager = new EditingContextManager();
            try
            {
                _ec = editingContextManager.GetEditingContext();//editingContextManager.GetOrCreateEditingContext(new CmsPage());
            }
            catch (Exception)
            {
                return;
            }
            
            //var activatedControl = CmsPageContentActivator.ActivateControl(toolboxItem,new Dictionary<string, string>());
            //if (Request["pageContentId"] != null)
            //{
               _existingContent = _ec.FindSubContentReursive(x => x.Id == new Guid(Request["pageContentId"])).Single().LocatedContent;
               _activatedControl = CmsPageContentActivator.ActivateControl(_existingContent);
            //}

            var defaultParameters = CmsPageContentActivator.GetContentParameterValues(_activatedControl);
            BuildEditorSurface(defaultParameters);
        }

        private void BuildEditorSurface(IDictionary<string,string> defaultParameters)
        {
            surface.Controls.Clear();
            foreach (var property in _toolboxItemType.GetProperties())
            {
                var displayNameDefinition = (DisplayNameAttribute) property.GetCustomAttribute(typeof(DisplayNameAttribute));
                var settingDefinition = (SettingAttribute) property.GetCustomAttribute(typeof(SettingAttribute));
                if (settingDefinition != null)
                {
                    var textbox = new ConfiguratorTextBox
                    {
                        PropertyName = property.Name,
                        DisplayName = property.Name,
                    };

                    if (displayNameDefinition != null)
                        textbox.DisplayName = displayNameDefinition.DisplayName;

                    textbox.Value = defaultParameters[property.Name];
                    surface.Controls.Add(textbox);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_existingContent == null)
                return;

            //if ("done" != ViewState["PageBuiltOnce"]?.ToString())
            //{
            //    var defaultParameters = CmsPageContentActivator.GetContentParameterValues(_activatedControl);
            //    BuildEditorSurface(defaultParameters);
            //    ViewState["PageBuiltOnce"] = "done";
            //}
            //else
            //{
                _existingContent.Parameters.Clear();
                foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
                {
                    _existingContent.Parameters.Add(tbx.PropertyName, tbx.Value);
                }

            //}

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (_ec == null)
                return;

            var js = new JavaScriptSerializer();
            this.WC_EDITING_CONTEXT_JSON.Value = js.Serialize(_ec);
        }
    }
}