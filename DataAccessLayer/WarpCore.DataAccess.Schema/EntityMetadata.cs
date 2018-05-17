using System;
using System.Collections.Generic;
using System.Text;

namespace WarpCore.Data.Schema
{
    public class EntityMetadata
    {
        public string TableName { get; set; }
        public string FriendlyName { get; set; }
        public EntityMetadata BaseMetadata { get; set; }

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
    }

}
