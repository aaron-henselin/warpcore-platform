<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavBar.ascx.cs" Inherits="DemoSite.NavBar" %>

<nav class="wc-navbar">
    <div class="container-fluid">
        <div class="wc-site-selector-icon">
            <span class="glyphicon glyphicon-globe"></span>
        </div>
        <div class="navbar-header">
            <asp:DropDownList class="pull-right wc-site-selector" runat="server" ID="SiteSelectorDropDownList" AutoPostBack="True"/>
        </div>
        <ul class="nav navbar-nav">
            
            
                <asp:Repeater runat="server" ID="NavBarRepeater" ItemType="DemoSite.NavBarItem">
                    <ItemTemplate>
                        <!-- no child items -->
                        <li runat="server" Visible="<%# !Item.ChildItems.Any() %>">
                            <a href="<%# Item.Url %>"><%# Item.Text %></a>
                        </li>
                        
                        <!-- has child items -->
                        <li runat="server" Visible="<%# Item.ChildItems.Any() %>" class="dropdown">
                            <a class="dropdown-toggle" data-toggle="dropdown" href="#">
                                <%# Item.Text %>
                                <span class="caret"></span>
                            </a>
                            <ul class="dropdown-menu">
                                <asp:Repeater runat="server" ID="SubItemRepeater" ItemType="DemoSite.NavBarItem" DataSource="<%# Item.ChildItems %>">
                                    <ItemTemplate>
                                        <li>
                                            <a href="<%# Item.Url %>"><%# Item.Text %></a>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </li>

                    </ItemTemplate>
                </asp:Repeater>   
            
            

        </ul>
    </div>
</nav>

<style>
    .wc-site-selector-icon{ 
        color: white;
        display: inline-block;
        left: 0;
        position: absolute;
        top: 18px;
    }
     .pagetree-item {
         padding-top: 5px;
         padding-bottom: 5px;
     }
    .wc-navbar {
        background-color: #2E2E2E;
    }
    .wc-navbar a {
        color: white;
    }
    .wc-site-selector {
        color: white;       
        background-color: #2E2E2E;
        border: 0;
        width: 100%;
        font-size: 18px;
        height: 50px;
    }
</style>