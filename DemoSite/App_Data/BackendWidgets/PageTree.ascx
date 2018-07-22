<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageTree.ascx.cs" Inherits="DemoSite.PageTree" %>

<h2>Page Tree</h2>
<asp:DropDownList runat="server" ID="SiteSelectorDropDownList" AutoPostBack="True"/>
<div runat="server" class="pagetree" ID="PageTreeWrapper">
    <asp:Repeater runat="server" ID="PageTreeItemRepeater" ItemType="DemoSite.PageTreeItem">
        <ItemTemplate>
            <div class="pagetree-item depth-<%# Item.Depth %>" 
                 data-path="<%# Item.SitemapNode.VirtualPath.ToString() %>"
                 data-parent="<%# Item.ParentItem %>">
                <span class="glyphicon glyphicon-triangle-right"></span>
                <span class="pagetree-item-title">
                    <%# Item.SitemapNode.Page.Name %>
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
                    <a href="javascript:void(0);">
                        <span class="glyphicon glyphicon-send"></span>
                        Publish
                    </a>
                    <a href="javascript:void(0);">
                        <span class="glyphicon glyphicon glyphicon-tasks"></span>
                        Settings
                    </a>
                   

                </span>
               

            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
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

    const containers = document.querySelectorAll('.pagetree');
    [].forEach.call(containers, function (el) {


        Sortable.create(el, {
            pull: true,
            put: true,
            sort:true,
            group: 'photo',
            animation: 150,
            draggable: ".pagetree-item", // Specifies which items inside the element should be sortable

        });
    });


</script>