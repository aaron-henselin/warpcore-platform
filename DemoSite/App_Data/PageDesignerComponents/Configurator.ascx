<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Configurator.ascx.cs" Inherits="DemoSite.Configurator1" %>
<%@ Import Namespace="DemoSite" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

<div class="designer wc-configurator">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            
            <h3 style="min-height: 50px;">
                <small class="pull-right">
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
        
            <div id="surface" runat="server">
                <a:RuntimeContentPlaceHolder ID="ConfiguratorFormBuilderRuntimePlaceHolder" PlaceHolderId="<%# ConfiguratorFormBuilder.RuntimePlaceHolderId %>" runat="server">

                </a:RuntimeContentPlaceHolder>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</div>    
