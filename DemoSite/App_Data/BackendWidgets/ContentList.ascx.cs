using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms;
using Cms.DynamicContent;
using Cms.Toolbox;
using Framework;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;

namespace DemoSite
{
    public class ContentListConfiguration : ISupportsJsonTypeConverter
    {
        public List<ContentListField> Fields { get; set; } = new List<ContentListField>();
    }

    public class ContentListField
    {
        public string Template { get; set; }
        public string Header { get; set; }
    }

    public class ContentListControlState
    {
        public List<ContentListField> Fields { get; set; } = new List<ContentListField>();
        public List<ContentListItem> Items { get; set; } = new List<ContentListItem>();
    }

    public class ContentListItem
    {
        public IDictionary<string, string> Values { get; set; }
    }


    public partial class ContentList : System.Web.UI.UserControl
    {
        [Setting]
        public Guid RepositoryId { get; set; }

        [Setting]
        public ContentListConfiguration Config { get; set; }

        //private ContentListControlState _controlState = new ContentListControlState();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);
            Reload();

            this.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
          
        }

        private void Reload()
        {
            var repoType = RepositoryTypeResolver.ResolveDynamicTypeByInteropId(RepositoryId);
            var repo = (IVersionedContentRepositoryBase) Activator.CreateInstance(repoType);
            var allDrafts = repo.FindContentVersions(string.Empty, ContentEnvironment.Draft).ToList();


            List<object> ds = new List<object>();
            var fields = Config.Fields;
            foreach (var d in allDrafts)
            {          
                var row = new List<string>();
                var dict1 = d.GetPropertyValues(x => true);
                foreach (var f in fields)
                {
                    var val = Templating.Interpolate(f.Template, dict1);
                    row.Add(val);
                }

                ds.Add(row);
            }

            var fieldConfigurations = new List<object>();
            foreach (var f in fields)
            {
                fieldConfigurations.Add(new { title = f.Header });
            }


            var js = new JavaScriptSerializer();
            Data.Value = js.Serialize(ds);
            Fields.Value = js.Serialize(fieldConfigurations);
            DataBind();

            //this.ContentListDataGrid.DataSource = ds;

            

            //this.ContentListDataGrid.AutoGenerateColumns = false;
            //foreach (var field in fields)
            //{
            //    this.ContentListDataGrid.Columns.Add(new BoundColumn { DataField = field, HeaderText = field });
            //}

            //this.ContentListDataGrid.DataBind();

            //var configFieldLookup = Configuration.Fields
            //    .Select(x => x.PropertyName)
            //    .ToDictionary(x => x, x => x);

            //foreach (var draft in allDrafts)
            //{
            //    var li = new ContentListItem
            //    {
            //        Values = draft.GetPropertyValues(x => configFieldLookup.ContainsKey(x.Name))
            //    };
            //    _controlState.Items.Add(li);
            //}
        }
    }
}