using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Cms.DynamicContent;
using Framework;
using WarpCore.Cms;
using WarpCore.DbEngines.AzureStorage;

namespace DemoSite
{
    [Serializable]
    public class EntityViewModel
    {
        public Guid TypeExtensionUid { get; set; }
        public string DisplayName { get; set; }
    }

    [Serializable]
    public class EntityListControlState
    {
        public List<EntityViewModel> Entities { get; set; } = new List<EntityViewModel>();
    }

    public partial class EntityList : System.Web.UI.UserControl
    {
        EntityListControlState _controlState = new EntityListControlState();

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);

            if (!Page.IsPostBack)
            {
                SetupControlState();
                FinishInit();
            }


        }

        private void FinishInit()
        {
            EntityRepeater.DataSource = _controlState.Entities;
            EntityRepeater.DataBind();
        }

        private void SetupControlState()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();


            var clrLookup = allTypes
                .HavingAttribute<SupportsCustomFieldsAttribute>()
                .Where(x => typeof(CosmosEntity).IsAssignableFrom(x))
                .ToLookup(x => x.GetCustomAttribute<SupportsCustomFieldsAttribute>().TypeExtensionUid);

            var repo = new TypeExtensionRepository();
            var allEntities = repo.Find()
                .Select(x => new EntityViewModel
                {
                    TypeExtensionUid = x.TypeResolverUid,
                    DisplayName = clrLookup[x.TypeResolverUid].FirstOrDefault()?.Name

                });

            _controlState.Entities = allEntities.ToList();
        }

        protected override void LoadControlState(object savedState)
        {
            _controlState = (EntityListControlState) savedState;
            FinishInit();
        }

        protected override object SaveControlState()
        {
            return _controlState;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
    }
}