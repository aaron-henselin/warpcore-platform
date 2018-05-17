using System;
using System.Collections.Generic;

namespace WarpCore.DbEngine
{
    public class DbRow : IRow
    {
        public DbTableSchema Schema { get; set; }

        public bool IsNew { get; set; }

        public static DbRow Create(DbTableSchema dbTableSchema, IDictionary<string,object> propertyValues)
        {
            
            var vs = new DbRow{Schema = dbTableSchema};
            foreach (var prop in dbTableSchema.Columns)
            {
                vs._values.Add(prop.ColumnName,propertyValues[prop.OriginatingPropertyName]);
            }
            return vs;
        }

  
        public object this[string columnName]
        {
            get
            {
                if (_values.ContainsKey(columnName))
                    return _values[columnName];

                throw new Exception($"A column does not exist with name '{columnName}'");
            }
        }

        private Dictionary<string, object> _values { get; set; } = new Dictionary<string, object>();


    }
}