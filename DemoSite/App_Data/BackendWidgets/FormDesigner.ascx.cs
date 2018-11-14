using Cms.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Orm;
using WarpCore.Web;
using WarpCore.Web.Extensions;
using static WarpCore.Web.CmsPageLayoutEngine;

namespace DemoSite
{
    public partial class FormDesigner : System.Web.UI.UserControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var formIdRaw = Request["formId"];
            var formTypeRaw = Request["formType"];
            ContentTypePicker.Visible = string.IsNullOrEmpty(formTypeRaw) && string.IsNullOrEmpty(formIdRaw);
            if (ContentTypePicker.Visible)
            {
                ContentTypeDropDownList.Items.Add(new ListItem(string.Empty, string.Empty));
                var all  = new RepositoryMetadataManager().Find();
                foreach (var contentType in all)
                {
                    var text = RepositoryTypeResolver.ResolveDynamicTypeByInteropId(new Guid(contentType.ApiId)).Name;
                    ContentTypeDropDownList.Items.Add(new ListItem(text,contentType.ApiId));
                }

                SelectContentTypeButton.Click += (sender, args) =>
                {
                    
                    //todo: make this work regardless of the querystring already present.
                    var url = HttpContext.Current.Request.RawUrl + "&formType=" + ContentTypeDropDownList.SelectedValue;
                   

                    HttpContext.Current.Response.Redirect(url);
                };
                return;
            }

            CmsForm cmsForm;
           
            if (string.IsNullOrEmpty(formIdRaw))
                cmsForm = new CmsForm
                {
                    ContentId = Guid.NewGuid(),
                    RepositoryUid = new Guid(formTypeRaw)
                    
                };
            else
            {
                var formRepository = new FormRepository();
                cmsForm = formRepository.FindContentVersions(By.ContentId(new Guid(formIdRaw)), ContentEnvironment.Draft).Result.Single();
            }

            var mgr = new EditingContextManager();
            var ec = mgr.GetOrCreateEditingContext(cmsForm);
            
            CmsPageLayoutEngine.ActivateAndPlaceContent(RuntimePlaceHolder, ec.AllContent,PageRenderMode.PageDesigner);

            RuntimePlaceHolder.Controls.AddAt(0, new DropTarget(RuntimePlaceHolder, DropTargetDirective.Begin));
            RuntimePlaceHolder.Controls.Add(new DropTarget(RuntimePlaceHolder, DropTargetDirective.End));

            this.Page.Init += (x,y) => this.Page.EnableDesignerDependencies();
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}