using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WarpCore.Platform.DataAnnotations
{
    public class DataRelationAttribute : Attribute
    {
        public string ApiId { get; set; }

        public DataRelationAttribute(string apiId)
        {
            ApiId = apiId;
        }


    }
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
    public interface IRequiresDataSource
    {
        Guid RepositoryApiId { get; set; }
        string DataSourceType { get; set; }

        DataSourceItemCollection Items { get; set; }

     
    }

    public interface ISupportsJavaScriptSerializer
    {

    }

   [DataContract]
    public class DataSourceItemCollection : ISupportsJavaScriptSerializer
    {
        public List<DataSourceItem> Items { get; set; } = new List<DataSourceItem>();
    }


    public class FixedOptionsDataSourceAttribute : Attribute
    {
        public string[] Options { get; }

        public FixedOptionsDataSourceAttribute(params string[] options)
        {
            Options = options;
        }
    }

    public static class DataSourceTypes
    {
        public const string Repository = nameof(Repository);
        public const string FixedItems = nameof(FixedItems);

    }
    [DataContract]
    public class DataSourceItem
    {
        public DataSourceItem()
        {
        }

        public DataSourceItem(string value)
        {
            Name = value;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class SerializedComplexObjectAttribute : Attribute
    {
    }

}
