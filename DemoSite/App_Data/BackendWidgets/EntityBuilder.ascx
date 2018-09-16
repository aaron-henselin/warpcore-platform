<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EntityBuilder.ascx.cs" Inherits="DemoSite.EntityBuilder" %>
<%@ Register TagPrefix="a" Namespace="DemoSite" Assembly="DemoSite" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Scripting" Assembly="WarpCore.Web" %>

<style>
    textarea {
        width: 100%;
        height: 100%;
    }


      
    
</style>

<a:ProxiedScriptManager runat="server"/> 
<div>

<asp:UpdatePanel runat="server">
    <ContentTemplate>

        <asp:Repeater runat="server" ItemType="DemoSite.DynamicPropertyViewModel" ID="DynamicPropertyRepeater">
            <ItemTemplate>
                <div class="row">
                    <div class="col-md-1">
                        <a:EntityBuilderActionBarPlaceHolder runat="server">
                            <div class="action-bar">
                                <asp:LinkButton runat="server" CommandArgument="<%# Item.Name %>" ID="RemoveProperty" OnClick="RemoveProperty_OnClick">
                                    <span class="glyphicon glyphicon-remove"></span>
                                </asp:LinkButton>
                                <asp:LinkButton runat="server" CommandArgument="<%# Item.Name %>" ID="EditProperty" OnClick="EditProperty_OnClick">
                                    <span class="glyphicon glyphicon-cog"></span>
                                </asp:LinkButton>
                            </div>
                        </a:EntityBuilderActionBarPlaceHolder>
                    </div>
                    <div class="col-md-6">
                        <%# Item.Name %>
                    </div>
                    <div class="col-md-4">
                        <%# Item.FieldType %>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <div class="row" runat="server" ID="BeginAddRow">
            <div class="col-md-11"></div>
            <div class="col-md-1">
                <asp:Button runat="server" Text="Add" CssClass="btn" ID="BeginAdd" OnClick="BeginAdd_OnClick"/>
            </div>
        </div>
        <asp:PlaceHolder runat="server" ID="PropertyAddFormWrapper" Visible="False">
            <div class="row" >
                <div class="col-md-12">
                    <div class="col-md-6">
                        <asp:Label runat="server" AssociatedControlID="PropertyNameTextBox">Property Name</asp:Label>
                        <asp:TextBox runat="server" ID="PropertyNameTextBox" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="col-md-2"></div>
                    <div class="col-md-4">
                        <asp:Label runat="server" AssociatedControlID="PropertyTypeDropDownList">Property Type</asp:Label>
                        <asp:DropDownList AutoPostBack="true" runat="server" ID="PropertyTypeDropDownList" CssClass="form-control"/>
                        <asp:PlaceHolder runat="server" ID="DataSourcePlaceHolder" Visible="False">
                            <asp:PlaceHolder runat="server" ID="UseRepositoryDataSourcePlaceHolder">
                                <asp:Label runat="server" AssociatedControlID="RepositoryDataSourceDropDownList" >
                                    Choice List
                                </asp:Label>
                                <asp:DropDownList runat="server" ID="RepositoryDataSourceDropDownList" CssClass="form-control"/>
                                <i>or</i>
                                <asp:LinkButton runat="server" ID="CreateCustomListLinkButton">
                                    create a custom list.
                                </asp:LinkButton>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" ID="UseCustomListDataSourcePlaceHolder">
                                
                            </asp:PlaceHolder>
                           

                        </asp:PlaceHolder>

                    </div>
                    <div class="col-md-12">
                        <asp:Label runat="server" AssociatedControlID="DescriptionTextBox">Description</asp:Label>
                        <asp:TextBox runat="server" ID="DescriptionTextBox" TextMode="MultiLine"></asp:TextBox>
                    </div>
        
                </div>
            </div>
            <div class="row">
                <div class="col-md-11"></div>
                <div class="col-md-1">
                    <asp:Button runat="server" CssClass="btn" Text="Cancel" ID="CancelButton" OnClick="CancelButton_OnClick"/>

                    <asp:Button runat="server" CssClass="btn" Text="Update" ID="SaveButton" OnClick="SaveButton_OnClick"/>
                </div>
            </div>
        </asp:PlaceHolder>
        
        <div class="row" runat="server" ID="FinishRow">
            <div class="col-md-11"></div>
            <div class="col-md-1">
                <asp:Button runat="server" ID="Button1" Text="Finish" CssClass="btn" OnClick="Finish_OnClick"/>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
 </div>