<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Configurator.ascx.cs" Inherits="DemoSite.Configurator1" %>
<%@ Import Namespace="WarpCore.Web.Widgets.FormBuilder.Support" %>


<div class="designer wc-configurator">
    <asp:UpdatePanel runat="server" ID="ConfiguratorSideBar">
        <ContentTemplate>
            <asp:Panel runat="server" Id="ConfiguratorSideBarBody">
            <h3 style="min-height: 50px;">
                <small class="float-right">
                    <button class="configurator-cancel btn">
                        <span class="glyphicon glyphicon-info-sign">
                        </span> Cancel
                    </button>
                    <asp:Button CssClass="btn" runat="server" Text="Save" ID="SaveButton"/>
                </small>
                Settings<br/>
                
            </h3>
            <wc-configurator-data style="display: none;">
                <asp:PlaceHolder runat="server" ID="DataBoundElements">


                    <input 
                        name="WC_CONFIGURATOR_CONTEXT_JSON" 
                        id="WC_CONFIGURATOR_CONTEXT_JSON"
                        value="<%# WC_CONFIGURATOR_CONTEXT_JSON %>"
                    />
                </asp:PlaceHolder>
                <asp:Button runat="server" ID="ConfiguratorInitButton" OnClick="ConfiguratorInitButton_OnClick" Text="Refresh" CssClass="wc-configurator-init-button"/>
            </wc-configurator-data>
        
            <asp:Panel runat="server" ID="surface" CssClass="wc-configurator-surface">
                <asp:PlaceHolder 
                    ID="ConfiguratorFormBuilderRuntimePlaceHolder"
                    PlaceHolderId="<%# ConfiguratorFormBuilder.RuntimePlaceHolderId %>" 
                    runat="server">

                </asp:PlaceHolder>
            </asp:Panel>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>    
