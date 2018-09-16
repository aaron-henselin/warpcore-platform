using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JsonSubTypes;
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

    public class SerializedComplexObjectAttribute : Attribute
    {
    }

    public interface ICosmosOrm
    {
        void Save(CosmosEntity item);

        Task<IReadOnlyCollection<T>> FindContentVersions<T>(string condition,
            ContentEnvironment version = ContentEnvironment.Live)
            where T : VersionedContentEntity, new();

        Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition)
            where T : UnversionedContentEntity, new();

        void Delete(CosmosEntity copy);
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
        Guid TypeExtensionUid { get; }
    }

    //public class DynamicVersionedContentBase : VersionedContentEntity
    //{
    //    public DynamicVersionedContentBase()
    //    {
    //    }
    //}
    
    [Table("orm_dynamic_object")]
    public class DynamicVersionedContent : VersionedContentEntity, IHasRuntimeTypeExtensionDefinition
    {
        private readonly Guid _uid;

        public DynamicVersionedContent(Guid uid)
        {
            _uid = uid;
        }

        Guid IHasRuntimeTypeExtensionDefinition.TypeExtensionUid => _uid;
    }

    public class DynamicUnversionedContent : UnversionedContentEntity, IHasRuntimeTypeExtensionDefinition
    {
        private readonly Guid _uid;

        public DynamicUnversionedContent(Guid uid)
        {
            _uid = uid;
        }

        Guid IHasRuntimeTypeExtensionDefinition.TypeExtensionUid => _uid;
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

    public class KnownChoiceTypes
    {
        public string TrueFalse = "TrueFalse";
        public string OptionList = "OptionList";
    }

    public class ChoiceFieldOption
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class ChoiceInterfaceFieldDataSource
    {
        public Guid FormInteropUid { get; set; }
        public string Filter { get; set; }

        public List<ChoiceFieldOption> Options { get; set; } = new List<ChoiceFieldOption>();
    }

    public class ChoiceInterfaceField : InterfaceField
    {
        public const string ContentFieldTypeUid = "Choice";
        public string ChoiceType { get; set; }
        public bool AllowMultipleSelections { get; set; }
        public ChoiceInterfaceFieldDataSource DataSource { get; set; }
        public ChoiceInterfaceField() : base(ContentFieldTypeUid)
        {
        }
    }

    public class TextInterfaceField : InterfaceField
    {
        public const string ContentFieldTypeUid = "Text";
        public bool IsHtml { get; set; }
        public int MaxLength { get; set; }
        public TextInterfaceField() : base(ContentFieldTypeUid)
        {
        }
    }
    
    internal interface IContentFieldTypeResolverMetadata
    {
        string ContentFieldType { get; }
    }

    [JsonConverter(typeof(JsonSubtypes), nameof(ContentFieldType))]
    [JsonSubtypes.KnownSubType(typeof(TextInterfaceField), TextInterfaceField.ContentFieldTypeUid)]
    [JsonSubtypes.KnownSubType(typeof(ChoiceInterfaceField), ChoiceInterfaceField.ContentFieldTypeUid)]
    public abstract class InterfaceField
    {
        protected InterfaceField(string contentFieldType)
        {
            ContentFieldType = contentFieldType;
        }
        public Guid PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyTypeName { get; set; }
        public string ContentFieldType { get; }
    }

    public class DynamicTypeDefinition
    {
        public List<InterfaceField> DynamicProperties { get; set; } =
            new List<InterfaceField>();
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


        public async void Delete(CosmosEntity item)
        {
            AssertIsOnline();

            var table = GetOrCreateTable(item.GetType()).Result;
            await table.ExecuteAsync(TableOperation.Delete(item));
        }

        public async void Save(CosmosEntity item)
        {
            AssertIsOnline();

            if (!item.IsDirty)
                return;

            if (item.InternalId == null)
                item.InternalId = Guid.NewGuid();

            var table = GetOrCreateTable(item.GetType()).Result;
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
            return await FindContentImpl<T>(condition);
        }

   
        

        private async Task<IReadOnlyCollection<T>> FindContentImpl<T>(string condition = null) where T : CosmosEntity, new()
        {
            AssertIsOnline();

            var cloudTable = GetOrCreateTable(typeof(T)).Result;
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
        



    }

}
