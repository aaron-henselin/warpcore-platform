<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FormDesigner.ascx.cs" Inherits="DemoSite.FormDesigner" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

<asp:PlaceHolder runat="server" ID="ContentTypePicker">
    <p>Get started building a brand new form. Select the repository whose content will be edited using this form.</p>
    <asp:DropDownList runat="server" ID="ContentTypeDropDownList" CssClass="form-control"/>
    <asp:Button runat="server" Text="Select" ID="SelectContentTypeButton" CssClass="btn btn-primary"/>
</asp:PlaceHolder>
    <a:RuntimeContentPlaceHolder ID="RuntimePlaceHolder" runat="server" PlaceHolderId="FormBody">
        
    </a:RuntimeContentPlaceHolder>
