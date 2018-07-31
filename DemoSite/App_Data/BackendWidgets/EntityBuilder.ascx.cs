using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms.Toolbox;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Web.Extensions;

namespace DemoSite
{
    [Serializable]
    public class DynamicPropertyViewModel
    {
        public string Name { get; set; }
        public string FieldType { get; set; }
    }

    [Serializable]
    public class EntityBuilderControlState
    {
        public bool IsAddingOrEditing { get; set; }
        public List<DynamicPropertyViewModel> Properties { get; set; } = new List<DynamicPropertyViewModel>();
        public string EditingId { get; set; }
    }

    public class EntityBuilderActionBarPlaceHolder : PlaceHolder
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
        }

        protected override object SaveControlState()
        {
            return _controlState;
        }



        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Page.RegisterRequiresControlState(this);

            if (Page.IsPostBack)
                return;

            var entityUiRaw = Request["wc-entity-uid"];
            if (!string.IsNullOrWhiteSpace(entityUiRaw))
            {
                var dynamicPropertyViewModels = GetAllDynamicFields(entityUiRaw);
                _controlState.Properties = dynamicPropertyViewModels;
            }

            FinishInit();
            
        }

        private static List<DynamicPropertyViewModel> GetAllDynamicFields(string entityUiRaw)
        {
            var md = new RepositoryMetadataManager().Find()
                .SelectMany(x => x.DynamicContentDefinitions)
                .First(x => x.EntityUid == entityUiRaw);

            var dynamicPropertyViewModels =
                md.DynamicProperties.Select(x => new DynamicPropertyViewModel
                {
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

        }

        protected void SaveButton_OnClick(object sender, EventArgs e)
        {
            var vm =new DynamicPropertyViewModel
            {
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
            throw new NotImplementedException();
        }
    }
}