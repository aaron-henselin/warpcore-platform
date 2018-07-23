<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Configurator.ascx.cs" Inherits="DemoSite.Configurator1" %>
<%@ Import Namespace="DemoSite" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

<div class="designer wc-configurator">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            
            <h3>
                Settings<br/>
                <small>
                    <button class="configurator-cancel">
                        <span class="glyphicon glyphicon-info-sign">
                        </span> Cancel
                    </button>

                    <%--                    <button class="configurator-save">
                        <span class="glyphicon glyphicon-info-sign">
                        </span> Save
                    </button>--%>
                    <asp:Button runat="server" Text="Save" ID="SaveButton"/>
                </small>
            </h3>

            <asp:PlaceHolder runat="server" ID="DataBoundElements">


                <input 
                    name="WC_CONFIGURATOR_CONTEXT_JSON" 
                    id="WC_CONFIGURATOR_CONTEXT_JSON"
                    value="<%# WC_CONFIGURATOR_CONTEXT_JSON %>"
                />
            </asp:PlaceHolder>
            <asp:Button runat="server" ID="ConfiguratorInitButton" OnClick="ConfiguratorInitButton_OnClick" Text="Refresh" CssClass="wc-configurator-init-button"/>
        
            <div id="surface" runat="server">
                <a:RuntimeContentPlaceHolder ID="ConfiguratorFormBuilderRuntimePlaceHolder" PlaceHolderId="<%# ConfiguratorFormBuilder.RuntimePlaceHolderId %>" runat="server">

                </a:RuntimeContentPlaceHolder>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</div>    
