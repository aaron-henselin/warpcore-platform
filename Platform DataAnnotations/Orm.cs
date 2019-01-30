using System;

namespace WarpCore.Platform.DataAnnotations
{

    public class TableAttribute : Attribute
    {
        public string TableName { get; }

        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }

    public class ColumnAttribute : Attribute { }
    

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
