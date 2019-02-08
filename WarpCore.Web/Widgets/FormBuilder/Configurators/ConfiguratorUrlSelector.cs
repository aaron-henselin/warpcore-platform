using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Orm;
using WarpCore.Web.Extensions;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.FormBuilder
{
    //[ToolboxItem(WidgetUid = ApiId, FriendlyName = "Url", Category = "Form Controls")]
    //public class ConfiguratorUrlSelector : PlaceHolder, INamingContainer, ILabeledConfiguratorControl
    //{
    //    public const string ApiId = "warpcore-formcontrol-urlselector";

    //    private Label _controlLabel = new Label { CssClass = "form-label" };
    //    private TextBox _externalUrlTextBox = new TextBox { CssClass = "form-control" };
    //    private DropDownList _internalUrlDropDownList = new DropDownList { CssClass = "form-control" };
    //    private CheckBox _selectExternalUrlCheckbox = new CheckBox { CssClass = "form-control", AutoPostBack = true, Text = "Reference a url outside of the content management system" };


    //    /// <summary>
    //    /// Sets which Property of the object should be configured using this textbox,
    //    /// when a value is submitted by an end user.
    //    /// </summary>
    //    [PropertiesAsOptionListSource(typeof(Uri))]
    //    [UserInterfaceHint(Editor = Editor.OptionList)]
    //    [DisplayName("Property name")]
    //    public string PropertyName { get; set; }

    //    /// <summary>
    //    /// Sets a custom label for the textbox when presented to the end user.
    //    /// </summary>
    //    [UserInterfaceHint]
    //    [DisplayName("Display name")]
    //    //[UserInterfaceBehavior(typeof(WhenPropertyNameChangedResetDisplayName))]
    //    public string DisplayName { get; set; }


    //    //[DisplayName("Site Filter")]
    //    //[UserInterfaceHint(Editor = Editor.OptionList)]
    //    //[DataRelation(SiteRepository.ApiId)]
    //    //public Guid? SiteId { get; set; }


    //    [UserInterfaceHint(Editor = Editor.Hidden)]
    //    public ConfiguratorBehaviorCollection Behaviors { get; set; } = new ConfiguratorBehaviorCollection();

    //    public string GetValue()
    //    {
    //        if (_selectExternalUrlCheckbox.Checked)
    //            return _externalUrlTextBox.Text;

    //        return _internalUrlDropDownList.SelectedValue;
    //    }


    //    public void InitializeEditingContext(ConfiguratorBuildArguments buildArguments)
    //    {
    //        //todo: rethink this.
    //        //////////////////////////////////////////////////
    //        Guid? SiteId=null;
    //        var dfr = HttpContext.Current.ToDynamicFormRequestContext();
    //        if (dfr.DefaultValues.ContainsKey(nameof(CmsPage.SiteId)))
    //            SiteId = new Guid(dfr.DefaultValues[nameof(CmsPage.SiteId)]);
    //        //////////////////////////////////////////////////
            
    //        var cmsPageRepository = new CmsPageRepository();
    //        var allDrafts = cmsPageRepository.FindContentVersions(null, ContentEnvironment.Draft).Result;

    //        _internalUrlDropDownList.Items.Clear();
    //        foreach (var draft in allDrafts)
    //        {
    //            if (SiteId == null || draft.SiteId == SiteId.Value)
    //            {
    //                var dataUri = new WarpCorePageUri(draft).ToDataUriString();
    //                _internalUrlDropDownList.Items.Add(new ListItem {Text = draft.Name, Value = dataUri});
    //            }
    //        }
    //    }

    //    public void SetConfiguration(SettingProperty settingProperty)
    //    {
    //        PropertyName = settingProperty.PropertyInfo.Name;
    //        DisplayName = settingProperty.DisplayName;
    //    }

    //    public void SetValue(string newValue)
    //    {
    //        if (string.IsNullOrWhiteSpace(newValue))
    //        {
    //            _selectExternalUrlCheckbox.Checked = false;
    //        }
    //        else
    //        {
    //            var uri = new Uri(newValue, UriKind.RelativeOrAbsolute);
    //            _selectExternalUrlCheckbox.Checked = uri.IsWarpCoreDataScheme();
    //        }

    //        RefreshVisibility();
    //    }

    //    protected override void OnInit(EventArgs e)
    //    {
    //        base.OnInit(e);
            
    //        this.Controls.Add(_controlLabel);
    //        this.Controls.Add(_internalUrlDropDownList);

    //        this.Controls.Add(_selectExternalUrlCheckbox);
    //        this.Controls.Add(_externalUrlTextBox);

    //        _controlLabel.Text = DisplayName;
    //    }

    //    protected override void OnPreRender(EventArgs e)
    //    {
    //        base.OnPreRender(e);

    //        RefreshVisibility();
    //    }

    //    private void RefreshVisibility()
    //    {
    //        _externalUrlTextBox.Visible = _selectExternalUrlCheckbox.Checked;
    //        _internalUrlDropDownList.Visible = !_selectExternalUrlCheckbox.Checked;
    //    }
    //}
}