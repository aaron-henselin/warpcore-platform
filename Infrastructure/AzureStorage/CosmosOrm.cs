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

        Task<IReadOnlyCollection<T>> FindContentVersions<T>(string condition,
            ContentEnvironment version = ContentEnvironment.Live)
            where T : VersionedContentEntity, new();

        Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition)
            where T : UnversionedContentEntity, new();

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

    interface IHasRuntimeTypeExtensionDefinition
    {
        string TypeExtensionUid { get; }
    }

    public class DynamicVersionedContent : VersionedContentEntity, IHasRuntimeTypeExtensionDefinition
    {
        private readonly string _uid;

        public DynamicVersionedContent(string uid)
        {
            _uid = uid;
        }

        string IHasRuntimeTypeExtensionDefinition.TypeExtensionUid => _uid;
    }

    public class DynamicUnversionedContent : UnversionedContentEntity, IHasRuntimeTypeExtensionDefinition
    {
        private readonly string _uid;

        public DynamicUnversionedContent(string uid)
        {
            _uid = uid;
        }

        string IHasRuntimeTypeExtensionDefinition.TypeExtensionUid => _uid;
    }

    public abstract class VersionedContentEntity : CosmosEntity
    {

        public VersionedContentEntity() : base(Dependency.Resolve<IDynamicTypeDefinitionResolver>())
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


        [JsonIgnore] public decimal ContentVersion { get; set; }

        [JsonIgnore]
        public ContentEnvironment ContentEnvironment
        {
            get => (ContentEnvironment) Enum.Parse(typeof(ContentEnvironment), PartitionKey);
            set => PartitionKey = value.ToString();
        }

    }

    public abstract class UnversionedContentEntity : CosmosEntity
    {
        public UnversionedContentEntity() : base(Dependency.Resolve<IDynamicTypeDefinitionResolver>())
        {
            this.PartitionKey = ContentEnvironment.Any.ToString();
        }
    }

    public class DynamicPropertyDescription
    {
        public string PropertyName { get; set; }
        public string PropertyTypeName { get; set; }

    }

    public class DynamicTypeDefinition
    {
        public List<DynamicPropertyDescription> DynamicProperties { get; set; } =
            new List<DynamicPropertyDescription>();
    }


    public interface IDynamicTypeDefinitionResolver
    {
        DynamicTypeDefinition Resolve(Type type);
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

        private async Task<CloudTable> GetOrCreateTable(Type type) 
        {
            var table = type.GetCustomAttribute<TableAttribute>();
            var tableRef = _client.GetTableReference(table.Name);
            if (!_collectionUris.ContainsKey(type))
            {
                await tableRef.CreateIfNotExistsAsync();
                _collectionUris.Add(type, tableRef.Uri);
            }

            return tableRef;
        }

        private async Task<CloudTable> GetOrCreateTable<T>() where T : CosmosEntity
        {
            return await GetOrCreateTable(typeof(T));
        }



    }

}
