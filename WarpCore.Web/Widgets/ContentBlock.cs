using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Cms;

namespace WarpCore.Web.Widgets
{
    [IncludeInToolbox(Name="WC/ContentBlock")]
    public class ContentBlock : Control
    {
        [Setting]
        public string AdHocHtml { get; set; }
    }
}