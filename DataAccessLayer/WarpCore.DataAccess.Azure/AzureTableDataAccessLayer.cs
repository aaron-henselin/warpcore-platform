using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace WarpCore.DbEngine.Azure
{
    public class AzureTableDataAccessLayer :IDbAdapter
    {
        private readonly AzureClientFactory _clientFactory;

        public AzureTableDataAccessLayer(AzureClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public async Task<ITable> Retrieve(DbTableSchema dbTableSchema)
        {
            var client = _clientFactory.CreateCloudTableClient();
            var table = client.GetTableReference(dbTableSchema.TableName);
            await CreateTableIfNotExistsAsync(dbTableSchema, table);
            
            var allColumns = dbTableSchema.Columns.Select(x => x.ColumnName).ToList();
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(allColumns);

            var tableResult = await FetchResults(dbTableSchema, table, projectionQuery);
            return tableResult;
        }

        public async Task<ITable> Retrieve(DbTableSchema dbTableSchema, Guid id)
        {
            var client = _clientFactory.CreateCloudTableClient();
            var table = client.GetTableReference(dbTableSchema.TableName);
            await CreateTableIfNotExistsAsync(dbTableSchema, table);
            
            var condition = TableQuery.GenerateFilterConditionForGuid("RowKey", QueryComparisons.Equal, id);
            var allColumns = dbTableSchema.Columns.Select(x => x.ColumnName).ToList();
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(allColumns).Where(condition);

            var tableResult = await FetchResults(dbTableSchema, table, projectionQuery);

            return tableResult;
        }

        private static async Task<ITable> FetchResults(DbTableSchema dbTableSchema, CloudTable table, TableQuery<DynamicTableEntity> projectionQuery)
        {
            // Initialize the continuation token to null to start from the beginning of the table.
            TableContinuationToken continuationToken = null;

            var tableResult = new AzureTable(dbTableSchema);

            do
            {
                // Retrieve a segment (up to 1,000 entities).
                var tableQueryResult =
                    await table.ExecuteQuerySegmentedAsync(projectionQuery, continuationToken);

                // Assign the new continuation token to tell the service where to
                // continue on the next iteration (or null if it has reached the end).
                continuationToken = tableQueryResult.ContinuationToken;

                foreach (var result in tableQueryResult.Results)
                    tableResult.AddRow(result);

                // Loop until a null continuation token is received, indicating the end of the table.
            } while (continuationToken != null);
            return tableResult;
        }

        public async void InsertOrUpdate(DbTableSchema dbTableSchema, IRow entityValueSet)
        {
            var client = _clientFactory.CreateCloudTableClient();
            var table = client.GetTableReference(dbTableSchema.TableName);
            await CreateTableIfNotExistsAsync(dbTableSchema, table);

            var dynamicRow = new DynamicTableEntity();
            foreach (var column in dbTableSchema.Columns)
            {
                var columnValue = entityValueSet[column.ColumnName];
                dynamicRow.Properties.Add(column.ColumnName, EntityProperty.CreateEntityPropertyFromObject(columnValue));
            }
            dynamicRow.RowKey = entityValueSet[dbTableSchema.PrimaryKeyColumnName]?.ToString();
            var insertTask = TableOperation.InsertOrReplace(dynamicRow);
            var insertResult = await table.ExecuteAsync(insertTask);
            if (insertResult.Result == null)
                throw new AzureDataAccessException("Table " + dbTableSchema.TableName + " could not be inserted to. ");
        }


        public async void Insert(DbTableSchema dbTableSchema, IRow entityValueSet)
        {
            InsertOrUpdate(dbTableSchema,entityValueSet);
        }

        public async void Update(DbTableSchema dbTableSchema, IRow entityValueSet)
        {
            InsertOrUpdate(dbTableSchema, entityValueSet);
        }


        private static async Task CreateTableIfNotExistsAsync(DbTableSchema dbTableSchema, CloudTable table)
        {
            await table.CreateIfNotExistsAsync();
        }
    }
}