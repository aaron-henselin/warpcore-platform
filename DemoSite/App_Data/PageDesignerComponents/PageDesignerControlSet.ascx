<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageDesignerControlSet.ascx.cs" Inherits="DemoSite.PageDesignerControlSet" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

    <a:AscxPlaceHolder UserControlId="PageDesignerState.ascx" runat="server" VirtualPath="/App_Data/PageDesignerComponents/PageDesignerState.ascx"></a:AscxPlaceHolder>
    <a:AscxPlaceHolder UserControlId="Configurator.ascx" runat="server" VirtualPath="/App_Data/PageDesignerComponents/Configurator.ascx"/>
    <a:AscxPlaceHolder UserControlId="Toolbox.ascx" runat="server" VirtualPath="/App_Data/PageDesignerComponents/Toolbox.ascx"/>
