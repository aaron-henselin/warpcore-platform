<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Toolbox.ascx.cs" Inherits="DemoSite.Toolbox" %>


<!-- toolbox (move to uc) -->
<div class="tbx">
    <div class="handle">
        <span class="glyphicon glyphicon-wrench"></span>
    </div>
    
    <h3>
        Toolbox<br/>
        <small>
            <span class="glyphicon glyphicon-info-sign">
            </span> Press <kbd>ESC</kbd> to close
        </small>
    </h3>
    <div id="toolboxUl" runat="server" class="toolbox-item-list">
        <asp:Repeater runat="server" ID="ToolboxItemRepeater" ItemType="DemoSite.ToolboxItemViewModel">
            <ItemTemplate>
                <div class="toolbox-item wc-layout-handle" data-wc-toolbox-item-name="<%# Item.WidgetTypeCode %>">
                    <%# Item.FriendlyName %>
                </div>

            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>