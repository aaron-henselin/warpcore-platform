<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageDesignerState.ascx.cs" Inherits="DemoSite.PageDesignerState" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Scripting" Assembly="WarpCore.Web" %>

<a:ProxiedScriptManager runat="server"></a:ProxiedScriptManager>
<asp:UpdatePanel runat="server" UpdateMode="Always">
    <ContentTemplate>
        <div runat="server" ID="EditingContextWrapper"></div>
    </ContentTemplate>
</asp:UpdatePanel>
