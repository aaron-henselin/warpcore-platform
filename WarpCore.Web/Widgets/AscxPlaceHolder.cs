using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WarpCore.Web.Widgets
{
    public class AscxPlaceHolder : PlaceHolder
    {
        public string VirtualPath { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var t = BuildManager.GetCompiledType(VirtualPath);
            var uc = (UserControl)Activator.CreateInstance(t);
            this.Controls.Add(uc);
        }
    }
}