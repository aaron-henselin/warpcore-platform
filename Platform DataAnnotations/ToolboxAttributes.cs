using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.DataAnnotations
{


    public class ToolboxItemAttribute : Attribute
    {
        public string WidgetUid { get; set; }
        public string FriendlyName { get; set; }
        public string Category { get; set; }
        public string AscxPath { get; set; }
        public bool UseClientSidePresentationEngine { get; set; }
    }

    public enum Editor
    {
        Text, RichText, OptionList, CheckBox,
        SubForm, Hidden,
        Url
    }

    public class IgnorePropertyAttribute : Attribute
    {
    }


}
