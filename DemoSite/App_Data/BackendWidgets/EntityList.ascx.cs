using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;
using WarpCore.Platform.Kernel.Extensions;
using WarpCore.Platform.Orm;

namespace DemoSite
{
    [Serializable]
    public class EntityViewModel
    {
        public Guid ContentTypeId { get; set; }
        public string DisplayName { get; set; }

        public List<EntityInterfaceViewModel> EditableInterfaces { get; set; } = new List<EntityInterfaceViewModel>();
    }

    [Serializable]
    public class EntityInterfaceViewModel
    {
        public Guid InterfaceId { get; set; }
        public string Name { get; set; }
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


            var extendableClrTypes = allTypes
                .HavingAttribute<WarpCoreEntityAttribute>()
                .Where(x => typeof(WarpCoreEntity).IsAssignableFrom(x));
                //.ToLookup(x => x.GetCustomAttribute<WarpCoreEntityAttribute>().TypeExtensionUid);

            List<EntityViewModel> vms = new List<EntityViewModel>();
            var allDynamicContentTypes = new ContentTypeMetadataRepository().Find();
            foreach (var t in allDynamicContentTypes)
            {
                vms.Add(new EntityViewModel{DisplayName = t.Name,ContentTypeId = t.ContentId});
            }

            foreach (var t in extendableClrTypes)
            {
                var contentTypeId = t.GetCustomAttribute<WarpCoreEntityAttribute>().TypeExtensionUid;
                vms.Add(new EntityViewModel { DisplayName = t.Name, ContentTypeId = contentTypeId });
            }


            var repo = new ContentInterfaceRepository();
            var contentInterfaceLookup = repo.Find().ToLookup(x => x.ContentTypeId);
            foreach (var entityViewModel in vms)
            {
                var appliedInterfaces = contentInterfaceLookup[entityViewModel.ContentTypeId];
                foreach (var contentInterface in appliedInterfaces)
                {
                    entityViewModel.EditableInterfaces.Add(new EntityInterfaceViewModel
                    {
                        InterfaceId = contentInterface.ContentId,
                        Name = contentInterface.InterfaceName
                    });
                }
            }

            _controlState.Entities = vms.ToList();

            //var repo = new ContentInterfaceRepository();
            //var allEntities = repo.Find()
            //    .Select(x => new EntityViewModel
            //    {
            //        ContentTypeId = x.ContentTypeId,
            //        DisplayName = clrLookup[x.ContentTypeId].FirstOrDefault()?.Name

            //    });

            //_controlState.Entities = allEntities.ToList();
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