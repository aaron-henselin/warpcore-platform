using System;

namespace WarpCore.Platform.DataAnnotations
{
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

        public Type CustomEditorType { get; set; }

    }

    public enum Editor
    {
        Text, RichText, OptionList, CheckBox,
        SubForm, Hidden,
        Url
    }

}
