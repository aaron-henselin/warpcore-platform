<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageTree.ascx.cs" Inherits="DemoSite.PageTree" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Scripting" Assembly="WarpCore.Web" %>

<h2>Page Tree</h2>
<a:ProxiedScriptManager runat="server"></a:ProxiedScriptManager>
<asp:DropDownList runat="server" ID="SiteSelectorDropDownList" AutoPostBack="True"/>

    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <div runat="server" class="pagetree" ID="PageTreeWrapper">
            <asp:Repeater runat="server" ID="PageTreeItemRepeater" ItemType="DemoSite.PageTreeItem">
                <ItemTemplate>
                    <asp:PlaceHolder runat="server" runat="server" Visible="<%# Item.Visible %>">
                          <div class="pagetree-item depth-<%# Item.Depth %>" 
                         data-path="<%# Item.VirtualPath.ToString() %>"
                         data-parent="<%# Item.ParentPath %>">
                        
                        <asp:LinkButton runat="server" 
                                        CommandArgument="<%# Item.VirtualPath.ToString() %>" 
                                        OnClick="ToggleExpandPageItem_OnClick" 
                                        ID="ExpandPageItem" 
                                        Visible="<%# Item.HasChildItems %>">
                         
                                <span runat="server" Visible="<%# !Item.IsExpanded %>" class="glyphicon glyphicon-triangle-right"></span>
                                <span runat="server" Visible="<%# Item.IsExpanded %>" class="glyphicon glyphicon-triangle-bottom"></span>
                        </asp:LinkButton>
                  
                        <span class="pagetree-item-title">
                            <%# Item.Name %>
                            <span runat="server" 
                                  Visible="<%# Item.IsHomePage %>"
                                  class="glyphicon glyphicon-home homepage-icon"></span>
                            <small class="unpublished badge" runat="server" Visible="<%# !Item.IsPublished %>">Draft</small>
                    
                        </span>
                        <span class="pull-right pagetree-item-actions">
                            <a href="<%# Item.DesignUrl %>">
                                <span class=" glyphicon glyphicon-text-background"></span>
                                Design
                            </a>
                            <asp:LinkButton runat="server"
                                ID="PublishLinkButton"
                                CommandArgument="<%# Item.PageId %>"
                                OnClick="PublishLinkButton_OnClick">
                                <span class="glyphicon glyphicon-send"></span>
                                Publish                                
                            </asp:LinkButton>
                            <a href="javascript:void(0);">
                                <span class="glyphicon glyphicon glyphicon-tasks"></span>
                                Settings
                            </a>
                   

                        </span>
               

                    </div>
                    </asp:PlaceHolder>
                  
                </ItemTemplate>
            </asp:Repeater>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>


<style>
    .unpublished.badge{ font-size: 10px;right:0}
    .homepage-icon {
        background-color: #009688;
        border-radius: 20px;
        color: white;
        font-size: 10px;
        padding: 5px;
        top: 0px;
    }

    .pagetree-item-actions {
        visibility: hidden;
        opacity: .25;
        transition: visibility 0s, opacity 0.5s linear;
    }

    .pagetree-item:hover
    .pagetree-item-actions {
        visibility: visible;
        opacity: 1;
    }
    .pagetree-item-title {
        
        padding-left: 10px; /* To remove default left padding */
        font-size: 18px;
        font-family: sans-serif;
    }
    .pagetree-item.depth-0{ padding-left: 30px;}
    .pagetree-item.depth-1{ padding-left: 60px;}
    .pagetree-item.depth-2{ padding-left: 90px;}
    .pagetree-item.depth-3{ padding-left: 120px;}
    .pagetree-item.depth-4{ padding-left: 150px;}
    .pagetree-item.depth-5{ padding-left: 180px;}

     .pagetree-item {
         border-bottom: 1px solid #efefef;
     }

   

</style>
<script>
    var pagetree = {};
    pagetree.init = function() {
        var containers = document.querySelectorAll('.pagetree');
        [].forEach.call(containers, function (el) {

            var alreadyInit = jQuery(el).attr("pagetree-init");
            if (alreadyInit)
                return;

            jQuery(el).attr("pagetree-init","true");

            Sortable.create(el, {
                pull: true,
                put: true,
                sort:true,
                group: 'photo',
                animation: 150,
                draggable: ".pagetree-item" // Specifies which items inside the element should be sortable

            });
        });
    }



</script>