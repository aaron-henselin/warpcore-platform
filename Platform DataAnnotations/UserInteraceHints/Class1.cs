using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.DataAnnotations.UserInteraceHints
{

    public enum Editor
    {
        Text, RichText, OptionList, CheckBox,
        SubForm, Hidden,
        Url,
        Static,
        Slug
    }

    public class IgnorePropertyAttribute : Attribute
    {
    }

    public class UserInterfaceIgnoreAttribute : Attribute
    {
    }

    public class DependsOnPropertyAttribute : Attribute
    {
        public string PropertyName { get; }

        public DependsOnPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    public class UserInterfaceBehaviorAttribute : Attribute
    {
        public Type BehaviorType { get; }

        public UserInterfaceBehaviorAttribute(Type behaviorType)
        {
            BehaviorType = behaviorType;
        }
    }

    public class UserInterfaceHintAttribute : Attribute
    {
        public Editor Editor { get; set; }

        public string CustomEditorType { get; set; }

    }
}
