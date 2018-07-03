<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Booting.aspx.cs" Inherits="WarpCore.Web.Booting" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            booting..
        </div>
    </form>
    <script>
        var continueExecution = function(){
            window.location.reload(true);
        };

        setTimeout(continueExecution, 1000);
    </script>
</body>
</html>
