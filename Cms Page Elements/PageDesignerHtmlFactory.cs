using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cms_PageDesigner_Context;
using Newtonsoft.Json;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Features.Presentation.Page.Elements
{
    //public class PageDesignerHtmlFactory
    //{
    //    private readonly IHttpRequest _httpRequest;
    //    private readonly IWebServer _webServer;

    //    public PageDesignerHtmlFactory(IHttpRequest httpRequest, IWebServer webServer)
    //    {
    //        _httpRequest = httpRequest;
    //        _webServer = webServer;
    //    }

    //    public string CreateLayoutHandle(PageCompositionElement pp)
    //    {
    //        var layoutHandle = $@"
    //                        <li class='StackedListItem StackedListItem--isDraggable wc-layout-handle' tabindex='1'
    //                            data-wc-page-content-id='{pp.ContentId}'>
    //                            <div class='StackedListContent'>
    //                                <h4 class='Heading Heading--size4 text-no-select'>

    //                                    <span class='glyphicon glyphicon-cog wc-edit-command configure'
    //                                        data-wc-widget-type='<%# HandleName %>' 
    //                                        data-wc-editing-command-configure='{pp.ContentId}'>
    //                                    </span>
    //                                    <span class='glyphicon glyphicon-remove wc-edit-command delete pull-right' 
    //                                         data-wc-editing-command-delete='{pp.ContentId}'>
    //                                    </span>
                                        
    //                                    {pp.FriendlyName}
    //                                </h4>
    //                                <div class='DragHandle'></div>
    //                                <div class='Pattern Pattern--typeHalftone'></div>
    //                                <div class='Pattern Pattern--typePlaced'></div>
    //                            </div>
    //                        </li>
    //                    ";

    //        return layoutHandle;
    //    }

    //    private string CreateHiddenHtml(string name, object value)
    //        {
    //            string serializedJson;
    //            if (value is string)
    //            {
    //                serializedJson = (string)value;
    //            }
    //            else
    //            {
    //                serializedJson = JsonConvert.SerializeObject(value);
    //                //var js = new JavaScriptSerializer();
    //                //serializedJson = js.Serialize(value);
    //            }

    //            var htmlVal = System.Net.WebUtility.HtmlEncode(serializedJson);
    //            return $"<input type='hidden' name='{name}' id='{name}' value='{htmlVal}'/>";
    //        }

    //    public IEnumerable<string> CreatePageDesignerStateTags()
    //    {
    //        var editingContext = Dependency.Resolve<EditingContextManager>().GetEditingContext();

    //        var hiddenHtml = CreateHiddenHtml(EditingContextVars.SerializedPageDesignStateKey, editingContext);
    //        yield return hiddenHtml;

    //        var clientSidePassthroughVariables = new[] {
    //            EditingContextVars.ClientSideConfiguratorStateKey,
    //            EditingContextVars.ClientSideToolboxStateKey };

    //        foreach (var clientSidePassthrough in clientSidePassthroughVariables)
    //        {
    //            var hiddenPassthrough = CreateHiddenHtml(clientSidePassthrough, _httpRequest.QueryString[clientSidePassthrough] ?? string.Empty);
    //            yield return hiddenPassthrough;
    //        }

    //    }

    //    public string LoadPageDesignerHead()
    //    {
    //        var path = _webServer.MapPath("/App_Data/PageDesignerComponents/Resources/pagedesigner-head.htm");
    //        return File.ReadAllText(path);
    //    }

    //    public string LoadPageDesignerStyle()
    //    {
    //        var path = _webServer.MapPath("/App_Data/PageDesignerComponents/Resources/pagedesigner.css");
    //        return "<style>"+File.ReadAllText(path)+"</style>";
    //    }

    //    public string LoadPageDesignerScripts()
    //    {
    //        var path= _webServer.MapPath("/App_Data/PageDesignerComponents/Resources/pagedesigner.js");
    //        return "<script>" + File.ReadAllText(path) + "</script>";

    //    }

    //    //protected override void OnPreRender(EventArgs e)
    //    //{
    //    //    base.OnPreRender(e);

    //    //    Literal l = new Literal { Text = string.Join("", CreatePageDesignerStateTags()) };

    //    //    EditingContextWrapper.Controls.Add(l);

    //    //    Controls.Add(new Button { ClientIDMode = ClientIDMode.Static, ID = EditingContextVars.EditingContextSubmitKey });
    //    //}
    //    public string CreateWidgetBeginMarker(PageCompositionElement pp)
    //    {
    //        return $"<wc-widget-render data-wc-layout='{pp.PlaceHolders.Any()}' data-wc-page-content-id='{pp.ContentId}'>";
    //    }

    //    public string CreateWidgetEndMarker(PageCompositionElement pp)
    //    {
    //        return "</wc-widget-render>";
    //    }

    //    public string CreateDropTargetBeginMarker(PageCompositionElement pp,RenderingsPlaceHolder relevantPlaceHolder)
    //    {
    //        return $"<wc-droptarget data-wc-placeholder-id='{relevantPlaceHolder.Id}' data-wc-layout-builder-id='{pp.LayoutBuilderId}'>";
    //    }

    //    public string CreateDropTargetEndMarker()
    //    {
    //        return "</wc-droptarget>";
    //    }
    //}
}
