using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace WarpCore.DbEngines.AzureStorage
{
    public class InMemoryDb : ICosmosOrm
    {
        private static DataSet _ds = new DataSet();

        private object _syncroot = new object();

        public void Save<T>(T item) where T : CosmosEntity
        {
            if (!item.IsDirty)
                return;

            var tableRef = GetOrCreateTable<T>();

            DataRow dr=null;
            if (item.RowKey != null)
                dr = tableRef.Select("RowKey = '" + item.RowKey + "'").SingleOrDefault();

            bool isNew = dr == null;
            if (isNew)
            {
                dr = tableRef.NewRow();
                item.RowKey = Guid.NewGuid().ToString();

                if (item.ContentId == Guid.Empty)
                    item.ContentId = Guid.NewGuid();
            }


            dr["RowKey"] = new Guid(item.RowKey);
            dr["PartitionKey"] = item.PartitionKey;

            var readProperties = typeof(T).GetProperties().Where(x =>
                !ShouldSkipProperty(x) && x.GetCustomAttribute<StoreAsComplexDataAttribute>() == null);

            foreach (var prop in readProperties)
                dr[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

            dr["ComplexData"] = item.ComplexData;

            if (isNew)
                tableRef.Rows.Add(dr);
        }

        public async Task<IReadOnlyCollection<T>> FindContentVersions<T>(string condition, ContentEnvironment version) where T : VersionedContentEntity, new()
        {
            string partitionCondition=null;
            if (version != ContentEnvironment.Any)
                partitionCondition= $"PartitionKey eq '{version}'";

            var allConditions = new[] {condition, partitionCondition}.Where(x => !string.IsNullOrWhiteSpace(x));
            var joinedCondition = string.Join(" and ", allConditions);

            return await Task.FromResult(FindContentImpl<T>(joinedCondition));
        }

        private IReadOnlyCollection<T> FindContentImpl<T>( string condition) where T : CosmosEntity, new()
        {
            var tableRef = GetOrCreateTable<T>();
            condition = ConvertToSql(condition);
            var rows = tableRef.Select(condition);

            List<T> activatedEntities = new List<T>();
            foreach (var row in rows)
            {
                var activated = new T();

                var writeProperties = typeof(T).GetProperties().Where(x =>
                    !ShouldSkipProperty(x) && x.GetCustomAttribute<StoreAsComplexDataAttribute>() == null);

                activated.RowKey = row["RowKey"]?.ToString();
                activated.PartitionKey = row["PartitionKey"]?.ToString();

                foreach (var prop in writeProperties)
                {
                    var objResult = row[prop.Name];
                    if (DBNull.Value == objResult)
                        objResult = null;
                    prop.SetValue(activated, objResult);
                }

                activated.InitializeChangeTracking();
                activatedEntities.Add(activated);
            }

            return activatedEntities;
        }


        public async Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition) where T : UnversionedContentEntity, new()
        {

            return await Task.FromResult(FindContentImpl<T>(condition));
        }

        public void Delete<T>(T copy) where T : CosmosEntity
        {
            var tableRef = GetOrCreateTable<T>();

            DataRow dr = tableRef.Select("RowKey = '" + copy.RowKey + "'").SingleOrDefault();
            if (dr != null)
                tableRef.Rows.Remove(dr);
        }

        private string ConvertToSql(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return condition;

            var conditionWords = condition.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < conditionWords.Length; i++)
            {
                if ("eq" == conditionWords[i])
                    conditionWords[i] = "=";

                if ("neq" == conditionWords[i])
                    conditionWords[i] = "<>";
            }

            return string.Join(" ", conditionWords);
        }

        private DataTable GetOrCreateTable<T>() where T : CosmosEntity
        {
            lock (_syncroot)
            {
                var tableAttribute = typeof(T).GetCustomAttribute<TableAttribute>();
                var tableRef = _ds.Tables.Cast<DataTable>().SingleOrDefault(x => x.TableName == tableAttribute.Name);
                if (tableRef != null)
                    return tableRef;

                var dataTable = new DataTable {TableName = tableAttribute.Name};

                dataTable.Columns.Add("RowKey", typeof(Guid));
                dataTable.Columns.Add("PartitionKey", typeof(string));

                var publicProperties = typeof(T).GetProperties().Where(x =>
                    !ShouldSkipProperty(x) && x.GetCustomAttribute<StoreAsComplexDataAttribute>() == null);

                foreach (var prop in publicProperties)
                {
                    bool allowNull = prop.PropertyType == typeof(string);

                    Type actualPropertyType = prop.PropertyType;
                    var isConstructedGeneric = prop.PropertyType.IsConstructedGenericType;
                    if (isConstructedGeneric && typeof(Nullable<>) == prop.PropertyType.GetGenericTypeDefinition())
                    {
                        actualPropertyType = prop.PropertyType.GetGenericArguments()[0];
                        allowNull = true;
                    }
                    

                    var newColumn = new DataColumn(prop.Name, actualPropertyType);
                    newColumn.AllowDBNull = allowNull;
                    dataTable.Columns.Add(newColumn);
                }

                _ds.Tables.Add(dataTable);
                return dataTable;
            }
        }

        internal static bool ShouldSkipProperty(PropertyInfo property)
        {
            string name = property.Name;
            if (name == "PartitionKey" || name == "RowKey" || (name == "Timestamp" || name == "ETag"))
                return true;

            MethodInfo setProp = property.SetMethod;
            MethodInfo getProp = property.GetMethod;
            if ((object)setProp == null || !setProp.IsPublic || ((object)getProp == null || !getProp.IsPublic))
            {
                return true;
            }
            if (setProp.IsStatic)
                return true;
            if (property.GetCustomAttribute(typeof(IgnorePropertyAttribute)) == null)
                return false;
            
            return true;
        }
    }
}