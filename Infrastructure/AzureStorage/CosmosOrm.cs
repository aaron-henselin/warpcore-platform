using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TinyIoC;

namespace WarpCore.DbEngines.AzureStorage
{
    public class CosmosDbConfiguration
    {

        //public static string ConnectionString => "DefaultEndpointsProtocol=https;AccountName=starfleetops;AccountKey=sLUhmnUL5siz2GfUQ981nBBfHyCvocdp6cK5bQUJaxlmfvhuABnBwJrMOpzpdtIXLizEFm3lovy5VfipTFOlKg==;TableEndpoint=https://starfleetops.table.cosmosdb.azure.com:443/;";



        public string ConnectionString { get; set; }

    }

    public class StoreAsComplexDataAttribute : Attribute
    {
    }

    public interface ICosmosOrm
    {
        void Save<T>(T item) where T : CosmosEntity;
        Task<IReadOnlyCollection<T>> FindContentVersions<T>(string condition, ContentEnvironment version = ContentEnvironment.Live) 
            where T : VersionedContentEntity, new();

        Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition)
            where T :UnversionedContentEntity, new();

        void Delete<T>(T copy) where T : CosmosEntity;
    }


    public enum ContentEnvironment
    {
        Any = -1,
        Draft = 0,
        Live = 1,
        Archive = 2
    }


    [Table("cms_content_checksum")]
    public class ContentChecksum : UnversionedContentEntity
    {
        public string ContentType { get; set; }
        public string Draft { get; set; }
        public string Live { get; set; }
    }

    public abstract class VersionedContentEntity : CosmosEntity
    {
        public VersionedContentEntity()
        {
            this.ContentEnvironment = ContentEnvironment.Draft;
        }

        public string GetContentChecksum()
        {
            var obj = JsonConvert.SerializeObject(this);
            var dict = JObject.Parse(obj);
            dict.Remove("RowKey");
            dict.Remove("ETag");
            dict.Remove("PartitionKey");
            dict.Remove("Timestamp");

            return dict.ToString();
        }
        

        [JsonIgnore]
        public decimal ContentVersion { get; set; }

        [JsonIgnore]
        public ContentEnvironment ContentEnvironment
        {
            get => (ContentEnvironment)Enum.Parse(typeof(ContentEnvironment), PartitionKey);
            set => PartitionKey = value.ToString();
        }

    }

    public abstract class UnversionedContentEntity : CosmosEntity
    {
        public UnversionedContentEntity()
        {
            this.PartitionKey = ContentEnvironment.Any.ToString();
        }
    }



    public abstract class CosmosEntity : TableEntity
    {
        [JsonIgnore]
        public bool IsNew => RowKey == null;
        

        /// <summary>
        /// An id unique to each environment, not shared between master and published copies
        /// </summary>
        [IgnoreProperty]
        public Guid? InternalId
        {
            get
            {
                if (RowKey == null)
                    return null;

                return new Guid(this.RowKey);
            }
            set { this.RowKey = value?.ToString(); }
        }

        /// <summary>
        /// An id unique to this content, but shared between master and published copies.
        /// </summary>
        public Guid? ContentId { get; set; }

        private string ChangeTracking { get; set; }

        internal void InitializeChangeTracking()
        {
            ChangeTracking = JsonConvert.SerializeObject(this);
        }

        internal bool IsDirty => InternalId == null || !string.Equals(ChangeTracking, JsonConvert.SerializeObject(this));

        public string ComplexData
        {
            get
            {
                var propsToSerialize = this.GetType()
                    .GetProperties()
                    .Where(x => x.GetCustomAttribute<StoreAsComplexDataAttribute>() != null
                             || x.PropertyType.GetCustomAttribute<StoreAsComplexDataAttribute>() != null  
                           );
                  

                var dataDict = new Dictionary<string, object>();
                foreach (var prop in propsToSerialize)
                {
                    dataDict.Add(prop.Name, prop.GetValue(this));
                }

                return JsonConvert.SerializeObject(dataDict);
            }
            set
            {
                var dataDict = JsonConvert.DeserializeObject(value, this.GetType());
                var propsToSerialize = this.GetType()
                    .GetProperties()
                    .Where(x => x.GetCustomAttribute<StoreAsComplexDataAttribute>() != null
                             || x.PropertyType.GetCustomAttribute<StoreAsComplexDataAttribute>() != null
                                );

                foreach (var prop in propsToSerialize)
                {
                    prop.SetValue(this, prop.GetValue(dataDict));
                }
            }
        }
    }

    
    

    public static class By
    {
        public static string ContentId(Guid contentId)
        {
            return $"{nameof(CosmosEntity.ContentId)} eq '{contentId}'";
        }
    }


    public class CosmosOrm : ICosmosOrm
    {
        private bool _offline = true;
        private CloudTableClient _client;

        private static Dictionary<Type, Uri> _collectionUris = new Dictionary<Type, Uri>();

        public CosmosOrm(CosmosDbConfiguration options)
        {
            var account = CloudStorageAccount.Parse(options.ConnectionString);
            _client = account.CreateCloudTableClient();
            _offline = false;
        }

        private void AssertIsOnline()
        {
            if (_offline)
                throw new Exception("Application is not connected to storage account.");
        }


        public async void Delete<T>(T item) where T : CosmosEntity
        {
            AssertIsOnline();

            var table = GetOrCreateTable<T>().Result;
            await table.ExecuteAsync(TableOperation.Delete(item));
        }

        public async void Save<T>(T item) where T : CosmosEntity
        {
            AssertIsOnline();

            if (!item.IsDirty)
                return;

            if (item.InternalId == null)
                item.InternalId = Guid.NewGuid();



            var table = GetOrCreateTable<T>().Result;
            await table.ExecuteAsync(TableOperation.InsertOrReplace(item));
        }

        public async Task<IReadOnlyCollection<T>> FindContentVersions<T>(string filter, ContentEnvironment version)
            where T : VersionedContentEntity, new()
        {

            string partitionCondition = null;
            if (version != ContentEnvironment.Any)
                partitionCondition = $"PartitionKey eq '{version}'";

            var allConditions = new[] {filter, partitionCondition}.Where(x => !string.IsNullOrWhiteSpace(x));
            var joinedCondition = string.Join(" and ", allConditions);

            return await FindContentImpl<T>(joinedCondition);
        }

        public async Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition) where T : UnversionedContentEntity, new()
        {
            //var search = $"PartitionKey eq '{ContentEnvironment.Any}'";
            //if (!string.IsNullOrWhiteSpace(condition))
            //    search += condition;

            return await FindContentImpl<T>(condition);
        }



        private async Task<IReadOnlyCollection<T>> FindContentImpl<T>(string condition = null) where T : CosmosEntity, new()
        {
            AssertIsOnline();

            var cloudTable = GetOrCreateTable<T>().Result;
            var query = new TableQuery<T> { FilterString = condition };
            var items = await cloudTable.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());

            var result = items.ToList();
            foreach (var item in result)
                item.InitializeChangeTracking();

            return result;
        }

        private async Task<CloudTable> GetOrCreateTable<T>() where T : CosmosEntity
        {
            var table = typeof(T).GetCustomAttribute<TableAttribute>();
            var tableRef = _client.GetTableReference(table.Name);
            if (!_collectionUris.ContainsKey(typeof(T)))
            {
                await tableRef.CreateIfNotExistsAsync();
                _collectionUris.Add(typeof(T), tableRef.Uri);
            }

            return tableRef;
        }



    }

}
