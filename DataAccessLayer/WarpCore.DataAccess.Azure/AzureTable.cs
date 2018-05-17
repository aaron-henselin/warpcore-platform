using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace WarpCore.DbEngine.Azure
{
    public class AzureTable :ITable
    {
        private readonly DbTableSchema _dbTableSchema;
        public ICollection<IRow> Rows { get; set; } = new List<IRow>();

        public AzureTable(DbTableSchema dbTableSchema)
        {
            _dbTableSchema = dbTableSchema;
        }

        public void AddRow(DynamicTableEntity result)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (var column in _dbTableSchema.Columns)
                values.Add(column.ColumnName, null);

            foreach (var property in result.Properties)
                try
                {
                    values[property.Key] = property.Value.PropertyAsObject;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Schema broken.");
                }

            var row = DbRow.Create(_dbTableSchema, values);
            
            Rows.Add(row);
        }

    }
}