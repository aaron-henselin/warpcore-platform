using Cms.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web;
using WarpCore.Web.Extensions;
using static WarpCore.Web.CmsPageDynamicLayoutBuilder;

namespace DemoSite
{
    public partial class FormDesigner : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            CmsForm cmsForm;
            var formIdRaw = Request["formId"];
            if (string.IsNullOrEmpty(formIdRaw))
                cmsForm = new CmsForm { ContentId = Guid.NewGuid()};
            else
            {
                var formRepository = new FormRepository();
                cmsForm = formRepository.FindContentVersions(By.ContentId(new Guid(formIdRaw)), ContentEnvironment.Draft).Result.Single();
            }

            var mgr = new EditingContextManager();
            var ec = mgr.GetOrCreateEditingContext(cmsForm);
            
            CmsPageDynamicLayoutBuilder.ActivateAndPlaceContent(RuntimePlaceHolder, ec.AllContent,PageRenderMode.PageDesigner);

            RuntimePlaceHolder.Controls.AddAt(0, new DropTarget(RuntimePlaceHolder, DropTargetDirective.Begin));
            RuntimePlaceHolder.Controls.Add(new DropTarget(RuntimePlaceHolder, DropTargetDirective.End));

            this.Page.Init += (x,y) => this.Page.EnableDesignerDependencies();
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}