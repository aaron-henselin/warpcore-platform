using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Cms.Toolbox;
using WarpCore.Web.Scripting;
using WarpCore.Web.Widgets;

namespace WarpCore.Web.Extensions
{

    public static class ControlExtensions
    {
        //public static void Add(this ControlCollection controlCollection, IFrontendComponent frontendComponent)
        //{
        //    controlCollection.Add((Control)frontendComponent);
        //}

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




        public static T FindDescendantControlOrSelf<T>(this Control parentControl, Func<T, bool> condition) where T : Control
        {
            var matchedType = parentControl as T;
            if (matchedType != null && condition((T) parentControl))
                return (T)parentControl;

            foreach (Control subControl in parentControl.Controls)
            {
                var subMatch = FindDescendantControlOrSelf<T>(subControl, condition);
                if (subMatch != null)
                    return subMatch;
            }

            return null;
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

        public static void EnableDesignerDependencies(this Page localPage)
        {
            var htmlForm = localPage.GetRootControl().GetDescendantControls<HtmlForm>().Single();
            htmlForm.Controls.Add(new ProxiedScriptManager());
            var bundle = new AscxPlaceHolder { VirtualPath = "/App_Data/PageDesignerComponents/PageDesignerControlSet.ascx" };
            htmlForm.Controls.Add(bundle);

            //localPage.PreLoad += (o, eventArgs) =>
            //{
                ScriptManagerExtensions.RegisterScriptToRunEachFullOrPartialPostback(localPage, "warpcore.page.edit();");
            //};
        }

        public static Control GetRootControl(this Page pageActual)
        {
            MasterPage topLevelMaster = pageActual.Master;
            if (topLevelMaster == null)
                return pageActual;

            while (topLevelMaster.Master != null)
                topLevelMaster = topLevelMaster.Master;

            return topLevelMaster;
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


        //public static List<ListItem> ToListItems<T, TValueProperty, TTextProperty>(this IEnumerable<T> items, Func<T, TValueProperty> valueProperty, Func<T, TTextProperty> textProperty, Func<T, bool> enabledProperty, DataSourceConfiguration config)
        //{
        //    //var valueMethod = valueProperty.Compile();
        //    //var textMethod = textProperty.Compile();

        //    var selectListItems = new List<ListItem> { };

        //    foreach (var item in items)
        //    {
        //        var value = valueProperty.Invoke(item)?.ToString();
        //        var text = textProperty.Invoke(item)?.ToString();

        //        bool enabled = true;
        //        if (enabledProperty != null)
        //            enabled = enabledProperty(item);

        //        selectListItems.Add(new ListItem(text, value, enabled));
        //    }
        //    if (config == null || !config.PreserveItemOrder)
        //        selectListItems = selectListItems.OrderBy(x => x.Text, StringComparer.OrdinalIgnoreCase).ToList();

        //    if (config == null || !config.ExcludeOptionLabel)
        //        selectListItems.Insert(0, new ListItem(config?.OptionLabelText, ""));


        //    return selectListItems;
        //}
        
    }

}