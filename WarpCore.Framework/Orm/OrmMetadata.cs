using System;
using System.Collections.Generic;
using System.Reflection;

namespace WarpCore.Framework
{
    public class OrmMetadata
    {
        public string TableName { get; set; }
        public string FriendlyName { get; set; }
        public OrmMetadata BaseMetadata { get; set; }

        public List<PropertyMetadata> Properties = new List<PropertyMetadata>();
    }

    public class PropertyMetadata
    {
        public Type PropertyType { get; set; }
        public string FriendlyName { get; set; }
        public string PropertyName { get; set; }

        public string PreferredEditor { get; set; }
        public bool PrimaryKey { get; set; }
        public string ColumnName { get; set; }
        public int? ColumnOrder { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }

}
