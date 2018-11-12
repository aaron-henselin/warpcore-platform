<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DynamicForm.ascx.cs" Inherits="DemoSite.DynamicForm" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

<h2>Add/Edit</h2>
<asp:PlaceHolder runat="server" ID="surface">
    <a:RuntimeContentPlaceHolder runat="server" PlaceHolderId="FormBody">
        
    </a:RuntimeContentPlaceHolder>
</asp:PlaceHolder>
<div>
    <asp:Button CssClass="btn" runat="server" ID="CancelButton" Text="Cancel" OnClick="CancelButton_OnClickButton_OnClick"/>
    <asp:Button CssClass="btn btn-primary" runat="server" ID="SaveButton" Text="Save" OnClick="SaveButton_OnClick"/>
</div>