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
            var row = new AzureRow(_dbTableSchema,result);
            Rows.Add(row);
        }

    }
}