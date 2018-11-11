<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FormDesigner.ascx.cs" Inherits="DemoSite.FormDesigner" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

<asp:PlaceHolder runat="server" ID="ContentTypePicker">
    <asp:DropDownList runat="server" ID="ContentTypeDropDownList"/>
    <asp:Button runat="server" ID="SelectContentTypeButton"/>
</asp:PlaceHolder>
    <a:RuntimeContentPlaceHolder ID="RuntimePlaceHolder" runat="server" PlaceHolderId="FormBody">
        
    </a:RuntimeContentPlaceHolder>
