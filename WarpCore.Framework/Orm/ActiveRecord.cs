using System;
using System.Collections.Generic;
using WarpCore.DbEngine;

namespace WarpCore.Framework.Orm
{
    public class ActiveRecord : IRow
    {
        public DbTableSchema Schema { get; set; }

        public static ActiveRecord Create(DbTableSchema dbTableSchema, IDictionary<string, object> propertyValues)
        {
            var vs = new ActiveRecord { Schema = dbTableSchema };
            foreach (var prop in dbTableSchema.Columns)
            {
                vs.Values.Add(prop.ColumnName, propertyValues[prop.OriginatingPropertyName]);
            }
            return vs;
        }

        public object this[string columnName]
        {
            get
            {
                if (Values.ContainsKey(columnName))
                    return Values[columnName];

                throw new Exception($"A column does not exist with name '{columnName}'");
            }
        }

        private Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();


    }
}