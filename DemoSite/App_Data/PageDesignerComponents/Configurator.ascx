<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Configurator.ascx.cs" Inherits="DemoSite.Configurator1" %>

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
            <div runat="server" ID="surface" class="surface">
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>
</div>    
