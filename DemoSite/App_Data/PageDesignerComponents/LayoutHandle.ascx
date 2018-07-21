<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LayoutHandle.ascx.cs" Inherits="DemoSite.LayoutHandle" %>


<li class="StackedListItem StackedListItem--isDraggable wc-layout-handle" tabindex="1"
    data-wc-page-content-id="<%# PageContentId %>">
    <div class="StackedListContent">
        <h4 class="Heading Heading--size4 text-no-select">

            <span class="glyphicon glyphicon-cog wc-edit-command configure"
                data-wc-widget-type="<%# HandleName %>" 
                data-wc-editing-command-configure="<%# PageContentId %>">
            </span>
            <span class='glyphicon glyphicon-remove wc-edit-command delete pull-right' 
                 data-wc-editing-command-delete="<%# PageContentId %>">
            </span>
            
            <%# HandleName %>
        </h4>
        <div class="DragHandle"></div>
        <div class="Pattern Pattern--typeHalftone"></div>
        <div class="Pattern Pattern--typePlaced"></div>
    </div>
</li>
