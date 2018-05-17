using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace WarpCore.DbEngine.Azure
{
    public class AzureRow 
    {
        public AzureRow(DbTableSchema dbTableSchema,DynamicTableEntity result)
        {
            foreach (var column in dbTableSchema.Columns)
                Columns.Add(column.ColumnName, null);

            foreach (var property in result.Properties)
                try
                {
                    Columns[property.Key] = property.Value.PropertyAsObject;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Schema broken.");
                }
        }

        private IDictionary<string,object> Columns { get;  set; } = new Dictionary<string, object>();
        
        public object this[string columnName] => Columns[columnName];
    }
}