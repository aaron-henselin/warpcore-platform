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

namespace DemoSite
{
    public class ConfiguratorTextBox :PlaceHolder
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Controls.Add(new Label { Text = DisplayName });
            this.Controls.Add(new TextBox());
        }
    }

    public partial class Configurator : System.Web.UI.Page
    {
        private EditingContext _ec;
        private Type _toolboxItemType;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Form.Action = HttpContext.Current.Request.RawUrl;

            var widgetType = Request["widgetType"];
            if (string.IsNullOrWhiteSpace(widgetType))
                return;

            var editingContextManager = new EditingContextManager();
            _ec = editingContextManager.GetOrCreateEditingContext(new CmsPage());

            //WC_EDITING_CONTEXT_JSON

            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(widgetType);
            _toolboxItemType = CmsPageContentActivator.ResolveToolboxItemType(toolboxItem);

            foreach (var property in _toolboxItemType.GetProperties())
            {
                var displayNameDefinition = (DisplayNameAttribute)property.GetCustomAttribute(typeof(DisplayNameAttribute));
                var settingDefinition = (SettingAttribute)property.GetCustomAttribute(typeof(SettingAttribute));
                if (settingDefinition != null)
                {
                    var textbox = new ConfiguratorTextBox
                    {
                        PropertyName = property.Name,
                        DisplayName =property.Name,
                    };

                    if (displayNameDefinition != null)
                        textbox.DisplayName = displayNameDefinition.DisplayName;

                    form1.Controls.Add(textbox);
                }
            }

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