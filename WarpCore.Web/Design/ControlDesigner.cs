using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WarpCore.Web.Design
{
    public class DesignedPropertyAttribute
    {
    }



    public class ControlDesigner
    {
        private void CreateDynamicLayout(Type controlType)
        {
            var allProperties = controlType.GetProperties();

        }


    }


}