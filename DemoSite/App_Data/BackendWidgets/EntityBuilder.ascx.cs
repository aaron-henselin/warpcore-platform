using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using Modules.Cms.Features.Presentation.RenderingEngines.WebForms;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Extensibility.DynamicContent;

namespace DemoSite
{
    [Serializable]
    public class DynamicPropertyViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FieldType { get; set; }
    }

    [Serializable]
    public class EntityBuilderControlState
    {
        public bool IsAddingOrEditing { get; set; }
        public List<DynamicPropertyViewModel> Properties { get; set; } = new List<DynamicPropertyViewModel>();
        public string EditingId { get; set; }
        public string PropertyType { get; set; }
    }

    public class EntityBuilderActionBarPlaceHolder : PlaceHolder
    {
    }

    public enum ContentFieldTopLevelType { Text,Choice,Number  }

    public class ContentFieldConfiguration
    {
    }

    public partial class EntityBuilder : System.Web.UI.UserControl
    {
        private EntityBuilderControlState _controlState = new EntityBuilderControlState();

        private void FinishInit()
        {
            DynamicPropertyRepeater.DataSource = _controlState.Properties;
            DynamicPropertyRepeater.DataBind();
            RefreshVisibility();
        }

        protected override void LoadControlState(object savedState)
        {
            _controlState = (EntityBuilderControlState)savedState;
            PropertyTypeDropDownList.SelectedValue = _controlState.PropertyType;
            FinishInit();
        }

        protected override object SaveControlState()
        {
            _controlState.PropertyType = PropertyTypeDropDownList.SelectedValue;
            return _controlState;
        }



        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);

            PropertyTypeDropDownList.Items.Add(ContentFieldTopLevelType.Text.ToString());
            PropertyTypeDropDownList.Items.Add(ContentFieldTopLevelType.Choice.ToString());

            var mgr = new RepositoryMetadataManager();
            var allRepos = mgr.Find();
            foreach (var repo in allRepos)
            {
                var repoType =
                RepositoryTypeResolver.ResolveTypeByApiId(repo.ApiId);

                var repoName = repo.CustomRepositoryName;
                if (string.IsNullOrWhiteSpace(repoName))
                    repoName = repoType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

                if (string.IsNullOrWhiteSpace(repoName))
                    repoName = repoType.Name;

                RepositoryDataSourceDropDownList.Items.Add(new ListItem(repoName,repo.ApiId.ToString()));
            }


            if (Page.IsPostBack)
                return;

            var interfaceId = Request["wc-interface"];
            if (!string.IsNullOrWhiteSpace(interfaceId))
            {
                var dynamicPropertyViewModels = GetAllDynamicFields(new Guid(interfaceId));
                _controlState.Properties = dynamicPropertyViewModels;
            }

            FinishInit();
            
        }

        private static List<DynamicPropertyViewModel> GetAllDynamicFields(Guid interfaceId)
        {
            var intf = new ContentInterfaceRepository().GetById(interfaceId);

            var dynamicPropertyViewModels =
                intf.InterfaceFields.Select(x => new DynamicPropertyViewModel
                {
                    Id=x.PropertyId,
                    Name = x.PropertyName,
                    FieldType = x.PropertyTypeName
                }).ToList();
            return dynamicPropertyViewModels;
        }


        protected void RemoveProperty_OnClick(object sender, EventArgs e)
        {
            var arg = ((LinkButton)sender).CommandArgument;

            var propToRemove = _controlState.Properties.Single(x => x.Name == arg);
            _controlState.Properties.Remove(propToRemove);

            FinishInit();
        }

        protected void EditProperty_OnClick(object sender, EventArgs e)
        {
            var arg = ((LinkButton) sender).CommandArgument;
            
            _controlState.IsAddingOrEditing = true;
            _controlState.EditingId = arg;

            var propToEdit = _controlState.Properties.Single(x => x.Name == arg);

            PropertyNameTextBox.Text = propToEdit.Name;
            PropertyTypeDropDownList.SelectedValue = propToEdit.FieldType;
        }

        protected void BeginAdd_OnClick(object sender, EventArgs e)
        {
            _controlState.IsAddingOrEditing = true;
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            var actionBars = this.GetDescendantControls<EntityBuilderActionBarPlaceHolder>();
            foreach (var actionBar in actionBars)
                actionBar.Visible = !_controlState.IsAddingOrEditing;

            PropertyAddFormWrapper.Visible = _controlState.IsAddingOrEditing;
            FinishRow.Visible = !_controlState.IsAddingOrEditing;

            DataSourcePlaceHolder.Visible = PropertyTypeDropDownList.SelectedValue == ContentFieldTopLevelType.Choice.ToString();

        }

        protected void SaveButton_OnClick(object sender, EventArgs e)
        {
            var vm =new DynamicPropertyViewModel
            {
                Id = Guid.NewGuid(),
                FieldType = PropertyTypeDropDownList.SelectedValue,
                Name = PropertyNameTextBox.Text
                
            };
            _controlState.Properties.Add(vm);
            _controlState.IsAddingOrEditing = false;
            
            FinishInit();
        }

        protected void CancelButton_OnClick(object sender, EventArgs e)
        {
            _controlState.IsAddingOrEditing = false;
            FinishInit();
        }

        protected void Finish_OnClick(object sender, EventArgs e)
        {
            var interfaceId = Request["wc-interface"];
            var intf = new ContentInterfaceRepository().GetById(new Guid(interfaceId));
            //intf.InterfaceFields = _controlState.Properties.Select(x=> new ChoiceInterfaceField
            //{
                

            //})

            //throw new NotImplementedException();
        }

        protected override void OnPreRender(EventArgs e)
        {
            RefreshVisibility();
        }
    }
}