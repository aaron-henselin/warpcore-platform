<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EntityList.ascx.cs" Inherits="DemoSite.EntityList" %>


<asp:Repeater runat="server" ID="EntityRepeater" ItemType="DemoSite.EntityViewModel">
    <ItemTemplate>
        <div class="row">
            <div class="col-md-6">
                <%# Item.DisplayName %>                
            </div>
            <div class="col-md-6">
                <a href="/Admin/entity-builder/?wc-entity-uid=<%# Item.TypeExtensionUid %>">
                    Customize Fields
                </a>
            </div>
            
        </div>
        
    </ItemTemplate>
</asp:Repeater>