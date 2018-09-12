<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentList.ascx.cs" Inherits="DemoSite.ContentList" %>

<table id="example">

</table>

<asp:HiddenField runat="server" ID="Data"/>
<asp:HiddenField runat="server" ID="Fields"/>

<script>

    var relatedId = "<%# Data.ClientID %>";
    var relatedFieldsId = "<%# Fields.ClientID %>";
    var jsonData = JSON.parse(jQuery("#" + relatedId).val());
    var jsonFields = JSON.parse(jQuery("#" + relatedFieldsId).val());

    jQuery('#example').DataTable({
        data: jsonData,
        columns: jsonFields
});

</script>