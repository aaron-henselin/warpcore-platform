using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Web;

namespace DemoSite
{


    public partial class PageDesignerState : System.Web.UI.UserControl
    {
        
        private string CreateHiddenHtml(string name, object value)
        {
            string serializedJson;
            if (value is string)
            {
                serializedJson = (string)value;
            }
            else
            {
                var js = new JavaScriptSerializer();
                serializedJson = js.Serialize(value);
            }

            return $"<input type='hidden' name='{name}' id='{name}' value='{Server.HtmlEncode(serializedJson)}'/>";
        }

        protected IEnumerable<string> CreatePageDesignerStateTags()
        {
            var editingContext = new EditingContextManager().GetEditingContext();

            var hiddenHtml = CreateHiddenHtml(EditingContextVars.SerializedPageDesignStateKey, editingContext);
            yield return hiddenHtml;

            var clientSidePassthroughVariables = new[] {
                EditingContextVars.ClientSideConfiguratorStateKey,
                EditingContextVars.ClientSideToolboxStateKey };

            foreach (var clientSidePassthrough in clientSidePassthroughVariables)
            {
                var hiddenPassthrough = CreateHiddenHtml(clientSidePassthrough, Page.Request[clientSidePassthrough] ?? string.Empty);
                yield return hiddenPassthrough;
            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Literal l = new Literal {Text = string.Join("", CreatePageDesignerStateTags())};

            EditingContextWrapper.Controls.Add(l);

            Controls.Add(new Button { ClientIDMode = ClientIDMode.Static, ID = EditingContextVars.EditingContextSubmitKey});
        }
    }
}