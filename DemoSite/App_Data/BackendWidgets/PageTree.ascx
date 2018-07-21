<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageTree.ascx.cs" Inherits="DemoSite.PageTree" %>

<h2>Page Tree</h2>
<asp:DropDownList runat="server" ID="SiteSelectorDropDownList" AutoPostBack="True"/>
<div runat="server" class="pagetree" ID="PageTreeWrapper">
    
</div>
<style>
    .pagetree ul,.pagetree li{
        list-style: none;
        margin: 0; /* To remove default bottom margin */ 
        padding: 0; /* To remove default left padding */


    }
    .pagetree li {
        padding-left: 10px;
       
    }
    .pagetree ul {
        min-height: 10px;
    }
    .pagetree-item-title {
        
        padding-left: 10px; /* To remove default left padding */
        font-size: 18px;
        font-family: sans-serif;
        border-bottom: 1px solid grey;
    }
     .pagetree-item-title {       

     }

   

</style>
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