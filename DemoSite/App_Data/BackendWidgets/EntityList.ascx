<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EntityList.ascx.cs" Inherits="DemoSite.EntityList" %>

<style>
    .row {
        padding-left: 30px;
        border-bottom: 1px solid #efefef;
        padding-top: 5px;
        padding-bottom: 5px;        
    }

</style>

<asp:Repeater runat="server" ID="EntityRepeater" ItemType="DemoSite.EntityViewModel">
    <ItemTemplate>
        <div class="row">
            <div class="col-md-6">
                <%# Item.DisplayName %>                
            </div>
            <div class="col-md-6">
                <asp:Repeater runat="server" ItemType="DemoSite.EntityInterfaceViewModel" DataSource="<%# Item.EditableInterfaces %>">
                    <ItemTemplate>
                        <a href="/Admin/entity-builder/?wc-interface=<%# Item.InterfaceId %>">
                            <%# Item.Name %>
                        </a>                        
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            
        </div>
        
    </ItemTemplate>
</asp:Repeater>