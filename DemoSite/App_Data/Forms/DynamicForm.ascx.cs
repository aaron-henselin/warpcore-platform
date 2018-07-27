using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.Forms;
using Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    public static class CmsFormReadWriter
    {
        public static void FillInControlValues(Control surface, IDictionary<string,string> entityValues)
        {
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
                tbx.Value = entityValues[tbx.PropertyName];
        }

        public static IDictionary<string, string> ReadValuesFromControls(Control surface)
        {
            Dictionary<string, string> newParameters = new Dictionary<string, string>();
            foreach (var tbx in surface.GetDescendantControls<ConfiguratorTextBox>())
            {
                newParameters.Add(tbx.PropertyName, tbx.Value);
            }
            return newParameters;
        }

    }



public partial class DynamicForm : System.Web.UI.UserControl
    {
        [Setting]
        public Guid FormId { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var formRepository = new FormRepository();
            var cmsForm = formRepository.FindContentVersions(By.ContentId(FormId),ContentEnvironment.Live).Result.Single();

            CmsPageDynamicLayoutBuilder.ActivateAndPlaceContent(surface, cmsForm.DesignedContent);

            var contentIdRaw = Request["contentId"];
            if (string.IsNullOrWhiteSpace(contentIdRaw))
            {

            }

            CmsFormReadWriter
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}