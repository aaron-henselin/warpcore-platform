<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Toolbox.ascx.cs" Inherits="DemoSite.Toolbox" %>


<!-- toolbox (move to uc) -->
<div class="tbx">
    <div class="handle">
        <span class="glyphicon glyphicon-wrench"></span>
    </div>
    <h3 class="save-header">
        <span class="button-group">
        <span class="button">
            <span class="glyphicon glyphicon-arrow-left">
                <asp:LinkButton runat="server" Text="Back" ID="BackToPageTreeLinkButton" OnClick="BackToPageTreeLinkButton_OnClick"></asp:LinkButton>
            </span>
            </span>
        </span>
        <span class="button-group right">
            <span class="button">
                <span class="glyphicon glyphicon-floppy-save">
                    <asp:LinkButton runat="server" Text="Save" ID="SaveDraftButton" OnClick="SaveDraftButton_OnClick"></asp:LinkButton>
                </span>
            </span>
            <span class="button last">
                <span class="glyphicon  glyphicon-send">
                <asp:LinkButton runat="server" Text="Publish" ID="SaveAndPublishButton" OnClick="SaveAndPublishButton_OnClick"></asp:LinkButton>
                </span>
            </span>
        </span>
    </h3>
    <h3>
        Toolbox<br/>
        <asp:DropDownList runat="server" ID="ToolboxCategorySelector"/>

<%--        <small>
            <span class="glyphicon glyphicon-info-sign">
            </span> Press <kbd>ESC</kbd> to close
        </small>--%>
    </h3>
    <div id="toolboxUl" runat="server" class="toolbox-item-list">
        <asp:Repeater runat="server" ID="ToolboxItemRepeater" ItemType="DemoSite.ToolboxItemViewModel">
            <ItemTemplate>

                <div class="toolbox-item wc-layout-handle" data-wc-toolbox-item-name="<%# Item.WidgetTypeCode %>">
                    <span class="glyphicon glyphicon-option-vertical">
                    </span>
                    <%# Item.FriendlyName %>
                </div>

            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>