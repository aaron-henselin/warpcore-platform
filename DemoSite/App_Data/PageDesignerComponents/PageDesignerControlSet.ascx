<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PageDesignerControlSet.ascx.cs" Inherits="DemoSite.PageDesignerControlSet" %>
<%@ Register TagPrefix="a" Namespace="WarpCore.Web.Widgets" Assembly="WarpCore.Web" %>

    <a:AscxPlaceHolder UserControlId="PageDesignerState.ascx" runat="server" VirtualPath="/App_Data/PageDesignerComponents/PageDesignerState.ascx"></a:AscxPlaceHolder>
    <a:AscxPlaceHolder UserControlId="Configurator.ascx" runat="server" VirtualPath="/App_Data/PageDesignerComponents/Configurator.ascx"/>
    <a:AscxPlaceHolder UserControlId="Toolbox.ascx" runat="server" VirtualPath="/App_Data/PageDesignerComponents/Toolbox.ascx"/>

<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous">

<script>
    var warpcore = {};
    if (typeof($) !== 'undefined')
        warpcore.jQueryRestore = $;
</script>
<script src="https://code.jquery.com/jquery-3.3.1.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/sortablejs@1.6.1/Sortable.min.js"></script>
<script src="/Scripts/jquery.slidereveal.min.js"></script>

<!-- Main Quill library -->
<script src="//cdn.quilljs.com/1.3.6/quill.js"></script>
<script src="//cdn.quilljs.com/1.3.6/quill.min.js"></script>

<!-- Theme included stylesheets -->
<link href="//cdn.quilljs.com/1.3.6/quill.snow.css" rel="stylesheet">
<link href="//cdn.quilljs.com/1.3.6/quill.bubble.css" rel="stylesheet">

<script>

    warpcore.jQuery = jQuery.noConflict();
    $ = warpcore.jQueryRestore;
</script>

<style>
    .tbx select {
        background-color: #2E2E2E;
        border: 0;
        width: 100%;
        font-size: 18px;
    }
     .tbx .toolbox-item {
         border-top: 1px dotted #3c3c3c;
         cursor: move;
         padding: 5px;
     }
     
    .wc-configurator{ background-color: #fff;}
    .wc-configurator-surface{ padding: 10px;}
    .wc-configurator .surface{ color: #000; background-color: #fff;}
    .wc-configurator .form-label{ color: #000000; font-weight:bold;}

    .configurator-embed{ width: 100%;height: 100%;}

    .tbx{ color: #fff;}
    .tbx .handle{
        
        background-color: #2E2E2E;
        right: -40px;
        padding: 10px;
        position: absolute;
        top: 0px;
        width: 40px;
        cursor: pointer;

    }

    /*.tbx {
        box-shadow: 10px 0px 15px 10px #585858;
    }*/

    .tbx h3 .button-group {
        top: 0;
        position: absolute;
        
    }

    .tbx h3 .button-group.right {
        right: 0;
    }

    .edit-with-rte{display:none;}
    
    .wc-configurator h3 {
        color: #fff;
    }
    .tbx h3,.wc-configurator h3 {
        background-color: #2E2E2E;
        font-size: 1.9em;
        padding: 5px;
        margin: 0;
        font-weight: 300;
      
    }

    .tbx h3.save-header {
        height: 40px;
        border-bottom: 1px solid #fff;
        filter: drop-shadow(0px 0px 10px rgba(0,0,0,.8));
    }
    .tbx h3.save-header .button a { color: white;}
    .tbx h3.save-header .button.last {
        border-left: 1px solid white;
    }
    .tbx h3.save-header .button {
        font-size: 10px;
        color:white;
       
        padding: 5px;
        margin-left: 2px;
    }

    .wc-layout-handle h4{ 
        font-family: Consolas,"Helvetica Neue",Helvetica,Arial,sans-serif;
        font-size: 14px;
    }
    .wc-layout-handle span.configure {
        margin-left: 5px;
    }
    .wc-layout-handle span.delete {
        margin-right: 2px;
    }
    .wc-layout-handle {
        display: block;
        width: 100%;
        background: #2E2E2E;
        border: #2E2E2E 1px solid;
        font-family: monospace;
        color: white;
        font-weight: bold;

        
    }
    .wc-droptarget-highlight div {
        border: 1px solid transparent;
        height: 50%;
    }
    .wc-droptarget-highlight.wc-droptarget-hover {
        height: 32px;
        margin-top: 0px;
        margin-bottom: 0px;

        transition-property: height;
        transition-duration: .5s;
        transition-property: margin-top;
        transition-duration: .5s;
        transition-property: margin-bottom;
        transition-duration: .5s;
    }
    .wc-droptarget-highlight.wc-droptarget-hover div {
        border: 1px dashed grey;
        transition-property: border;
        transition-duration: 1s;

    }
    wc-droptarget {
        display: block;
        min-height: 100px;
        border: 1px dashed #efefef;
        transition-property: height;
        transition-duration: 1s;
    }

    div .wc-droptarget-highlight:last-child {
        margin-top: 10px;
    }
    .wc-droptarget-highlight {
        height: 32px;
        transition-property: height;
        transition-duration: 1s;
        margin-top: -8px;
        margin-bottom: -32px;
        z-index: 10000;
        position: relative;
    }


</style>


<script>

    configurator_submit = function () {
        debugger;
        jQuery('#WC_EDITING_SUBMIT').click();
    };

    warpcore.page = {};
    
    warpcore.page.edit = function () {

        var $ = warpcore.jQuery;

        $('.edit-with-rte').each(function() {

            var $this = $(this);
            var rteInputId = $this.attr("id");
            var $rteSurface = $("[data-rte-for='" + rteInputId + "']");

            if (!$rteSurface.hasClass("data-rte-initialized")) {
                $rteSurface.addClass("data-rte-initialized");

                var toolbarOptions = [
                    ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
                    ['blockquote', 'code-block'],

                    [{ 'header': 1 }, { 'header': 2 }],               // custom button values
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                    [{ 'script': 'sub'}, { 'script': 'super' }],      // superscript/subscript
                    [{ 'indent': '-1'}, { 'indent': '+1' }],          // outdent/indent
                    [{ 'direction': 'rtl' }],                         // text direction

                    [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
                    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],

                    [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
                    [{ 'font': [] }],
                    [{ 'align': [] }],

                    ['clean']                                         // remove formatting button
                ];

               

                var quill = new Quill("[data-rte-for='" + rteInputId + "']",
                    {
                        modules: {
                            toolbar: toolbarOptions
                        },
                        theme: 'snow'
                    });
                quill.setContents($this.val());
                quill.on('text-change', function(delta, oldDelta, source) {
                    $this.val(quill.getText());
                });
            }
              

        });

        if (window.once === true)
            return;

        window.once = true;



        //$("body > *").wrapAll("<wc-page-preview/>");


        $(".configurator-cancel").click(function() {});
        


        $('.wc-configurator').slideReveal({ overlay: true, speed: 0, position: "right", push: false, width: "550px"});
        $("[data-wc-editing-command-configure]").click(function() {
            var configureContentId = $(this).data("wc-editing-command-configure");
            var configureContentType = $(this).data("wc-widget-type");

            $('.tbx').slideReveal("hide");

            var newConfiguratorContextJson = JSON.stringify({
                IsOpening: true,
                PageContentId: configureContentId
            });
            $("#WC_CONFIGURATOR_CONTEXT_JSON").val(newConfiguratorContextJson);

            $(".wc-configurator-init-button").click();
            $('.wc-configurator').slideReveal("show");

        });
        

        $('.tbx').slideReveal({
            trigger: $(".tbx .handle"),
            speed: 0,
            shown: function(obj){
                obj.find(".handle").html('<span class="glyphicon glyphicon-chevron-left"></span>');
                obj.addClass("right-shadow-overlay");
                $("#WC_TOOLBOX_STATE").val(JSON.stringify({ isOpen: true }));
            },
            hidden: function(obj){
                obj.find(".handle").html('<span class="glyphicon glyphicon-wrench"></span>');
                obj.removeClass("right-shadow-overlay");
                $("#WC_TOOLBOX_STATE").val(JSON.stringify({ isOpen: false }));
            }
        });

        var tbxStateRaw = $("#WC_TOOLBOX_STATE").val();
        if (tbxStateRaw) {
            var tbxState = JSON.parse(tbxStateRaw);
            var isOpen = tbxState && tbxState.isOpen;
            if (isOpen)
                $('.tbx').slideReveal("show");
        }


        var container = document.querySelectorAll("body")[0];


        //var sort = Sortable.create(container, {
        //    animation: 150, // ms, animation speed moving items when sorting, `0` — without animation
        //    handle: ".wc-layout-handle", // Restricts sort start click/touch to the specified element
        //    draggable: ".wc-layout-handle", // Specifies which items inside the element should be sortable

        //});

        const containers = document.querySelectorAll('wc-droptarget,.toolbox-item-list');
        [].forEach.call(containers, function (el) {

            var pull = true;
            var put = true;
            var sort = true;

            var isToolbox = $(el).hasClass("toolbox-item-list");
            if (isToolbox) {
                pull = "clone";
                put = false;
                sort = false;
            }


            Sortable.create(el, {
                pull: pull,
                put: put,
                sort:sort,
                group: 'photo',
                animation: 150,
                filter: '.wc-edit-command',
                handle: ".wc-layout-handle", // Restricts sort start click/touch to the specified element
                draggable: ".wc-layout-handle", // Specifies which items inside the element should be sortable

                onChoose: function(evt) {
                    var matchId = $(evt.item).data("wc-page-content-id");
                    $("wc-widget-render[data-wc-page-content-id='" + matchId + "']").hide();
                    $("wc-widget-render[data-wc-layout='False']").slideUp();
                    
                },
                onMove: function (evt/**Event*/) {


                    //var item = evt.item; // the current dragged HTMLElement
                    //drop(item);
                },
                onUpdate: function (evt/**Event*/){
                    drop(evt.item,evt.to);
                },
                onAdd: function (evt/**Event*/){
                    drop(evt.item,evt.to);
                }
            });
        });

        $("[data-wc-editing-command-delete]").click(function() {

            var moveContentId = $(this).data("wc-editing-command-delete");

            $("wc-widget-render[data-wc-page-content-id=" + moveContentId + "]").hide();
            $("wc-layout-handle[data-wc-page-content-id=" + moveContentId + "]").hide();

            var deleteCommand =
            {
                "PageContentId": moveContentId
            };
            deleteCommand.EditingContext = JSON.parse($("#WC_EDITING_CONTEXT_JSON").val());
            var deleteCommandJson = JSON.stringify(deleteCommand);


            $.ajax({
                type: "POST",
                url: "/wc-api/pagedesigner/Delete",
                // The key needs to match your method's input parameter (case-sensitive).
                data: deleteCommandJson,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function(data) {

                    var str = JSON.stringify(data);
                    $("#WC_EDITING_CONTEXT_JSON").val(str);
                    $("#WC_EDITING_SUBMIT").click();

                },
                failure: function(errMsg) {
                    alert(errMsg);
                }
            });

        });



        var drop = function(el,to) {


            var addContentName = $(el).data("wc-toolbox-item-name");
            var moveContentId = $(el).data("wc-page-content-id");
            var $to = $(to);



            var $detached = $("wc-widget-render[data-wc-page-content-id=" + moveContentId + "]").detach();
            $(el).after($detached);
            $("wc-widget-render[data-wc-page-content-id=" + moveContentId + "]").show();
            $("wc-widget-render[data-wc-layout='False']").slideDown();

            if (moveContentId) {
                var moveCommand =
                {
                    "PageContentId": moveContentId,
                    "ToContentPlaceHolderId": $to.data("wc-placeholder-id"),
                    "ToLayoutBuilderId": $to.data("wc-layout-builder-id"),
                    "BeforePageContentId": $to.data("wc-before-page-content-id"),
                };
                moveCommand.EditingContext = JSON.parse($("#WC_EDITING_CONTEXT_JSON").val());

                var moveCommandJson = JSON.stringify(moveCommand);
                //$("#WC_EDITING_MOVE_COMMAND").val(moveCommandJson);
                //$("#WC_EDITING_SUBMIT").click();

                $.ajax({
                    type: "POST",
                    url: "/wc-api/pagedesigner/Move",
                    // The key needs to match your method's input parameter (case-sensitive).
                    data: moveCommandJson,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(data) {

                        var str = JSON.stringify(data);
                        $("#WC_EDITING_CONTEXT_JSON").val(str);
                        $("#WC_EDITING_SUBMIT").click();

                    },
                    failure: function(errMsg) {
                        alert(errMsg);
                    }
                });


            } else {

                var addCommand =
                {
                    "WidgetType": addContentName,
                    "ToContentPlaceHolderId": $to.data("wc-placeholder-id"),
                    "ToLayoutBuilderId": $to.data("wc-layout-builder-id"),
                    "BeforePageContentId": $to.data("wc-before-page-content-id")
                };
                addCommand.EditingContext = JSON.parse($("#WC_EDITING_CONTEXT_JSON").val());
                var addCommandJson = JSON.stringify(addCommand);
                $.ajax({
                    type: "POST",
                    url: "/wc-api/pagedesigner/Add",
                    // The key needs to match your method's input parameter (case-sensitive).
                    data: addCommandJson,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(data) {

                        var str = JSON.stringify(data);
                        $("#WC_EDITING_CONTEXT_JSON").val(str);
                        $("#WC_EDITING_SUBMIT").click();

                    },
                    failure: function(errMsg) {
                        alert(errMsg);
                    }
                });


            }

        }


    }
    
</script>
    

