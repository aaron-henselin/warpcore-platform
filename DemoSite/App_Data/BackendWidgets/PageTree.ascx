<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageTree.ascx.cs" Inherits="DemoSite.PageTree" %>

<h2>Page Tree</h2>
<asp:DropDownList runat="server" ID="SiteSelectorDropDownList" AutoPostBack="True"/>
<div runat="server" class="pagetree" ID="PageTreeWrapper">
    
</div>

<script>

    const containers = document.querySelectorAll('.pagetree ul');
    [].forEach.call(containers, function (el) {


        Sortable.create(el, {
            pull: true,
            put: true,
            sort:true,
            group: 'photo',
            animation: 150,
            draggable: "li", // Specifies which items inside the element should be sortable

        });
    });


</script>