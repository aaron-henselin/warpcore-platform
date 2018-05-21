using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WarpCore.Web.Extensions
{
    public static class ControlExtensions
    {


        public static bool ContainsValue(this ListItemCollection collection, string value)
        {
            return collection.Cast<ListItem>().Any(x => string.Equals(value, x.Value));
        }

        public static void RegisterDescendentAsyncPostBackControl(Control control)
        {

            var handlers1 = control.GetDescendantControls<Control>()
                .OfType<IPostBackEventHandler>().Cast<Control>();

            var handlers2 = control.GetDescendantControls<Control>()
                .OfType<IPostBackDataHandler>().Cast<Control>();

            var handlers = handlers1.Union(handlers2);

            //.Where(x => x is IPostBackEventHandler);
            foreach (var handler in handlers)
            {
                var sm = ScriptManager.GetCurrent(control.Page);
                sm.RegisterAsyncPostBackControl(handler);
            }
        }

        public static IEnumerable<T> GetDescendantControls<T>(this Control parentControl) where T : Control
        {
            return ControlExtensions.GetDescendantControls(parentControl, typeof(T)).Cast<T>();
        }

        public static IEnumerable<Control> GetDescendantControls(this Control parentControl, Type t)
        {
            if (parentControl.HasControls())
                foreach (Control control in parentControl.Controls)
                {

                    if (t.IsInstanceOfType(control))
                        yield return control;

                    foreach (var childControl in GetDescendantControls(control, t))
                    {
                        yield return childControl;
                    }
                }
        }

        public static IEnumerable<T> GetAnscestorControls<T>(this Control childControl) where T : Control
        {
            while (childControl.Parent != null)
            {
                var parentControl = childControl.Parent;

                if (parentControl is T)
                    yield return (T)parentControl;

                childControl = childControl.Parent;
            }

        }

        


        public static void SetDataSource(this ListControl dataBoundControl, List<ListItem> selectListItems)
        {
            var selectedValue = dataBoundControl.SelectedValue;
            dataBoundControl.Items.Clear();
            dataBoundControl.Items.AddRange(selectListItems.ToArray());

            if (selectListItems.Any(x => x.Value == selectedValue))
                dataBoundControl.SelectedValue = selectedValue;
            else
                dataBoundControl.SelectedValue = null;
        }


    }

}