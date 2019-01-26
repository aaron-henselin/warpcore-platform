using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace WarpCore.Web.Widgets.Content
{
    [CompositeConfiguratorType]
    public class ComplexDemo
    {
        public string Test1 { get; set; }
        public string Test2 { get; set; }
    }


}