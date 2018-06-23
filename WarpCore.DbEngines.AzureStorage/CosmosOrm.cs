using System;
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
using TinyIoC;

namespace WarpCore.DbEngines.AzureStorage
{
    public class CosmosDbConfiguration
    {

        //public static string ConnectionString => "DefaultEndpointsProtocol=https;AccountName=starfleetops;AccountKey=sLUhmnUL5siz2GfUQ981nBBfHyCvocdp6cK5bQUJaxlmfvhuABnBwJrMOpzpdtIXLizEFm3lovy5VfipTFOlKg==;TableEndpoint=https://starfleetops.table.cosmosdb.azure.com:443/;";



        public string ConnectionString { get; set; }

    }

    public class ComplexDataAttribute : Attribute
    {
    }

    public interface ICosmosOrm
    {
        void Save<T>(T item) where T : CosmosEntity;
        Task<IReadOnlyCollection<T>> FindContentVersions<T>(Guid contentId, ContentEnvironment version = ContentEnvironment.Live) where T : CosmosEntity, new();
        void Delete<T>(T copy) where T : CosmosEntity;
    }


    public enum ContentEnvironment
    {
        Unversioned = -1,
        Draft = 0,
        Live = 1,
        Archive = 2
    }

    public abstract class CosmosEntity : TableEntity
    {
        public CosmosEntity() : base(null, null)
        {
            var isVersioned = !this.GetType().GetCustomAttributes<UnversionedAttribute>().Any();
            if (isVersioned)
                this.ContentEnvironment = ContentEnvironment.Draft;
            else
                this.ContentEnvironment = ContentEnvironment.Unversioned;
            
        }

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

        public decimal ContentVersion { get; set; }

        public ContentEnvironment ContentEnvironment {
            get => (ContentEnvironment)Enum.Parse(typeof(ContentEnvironment),PartitionKey);
            set => PartitionKey = value.ToString();
        } 

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
                var propsToSerialize = this.GetType().GetProperties().Where(x => x.GetCustomAttribute<ComplexDataAttribute>() != null);
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
                var propsToSerialize = this.GetType().GetProperties().Where(x => x.GetCustomAttribute<ComplexDataAttribute>() != null);
                foreach (var prop in propsToSerialize)
                {
                    prop.SetValue(this, prop.GetValue(dataDict));
                }
            }
        }
    }

    public sealed class UnversionedAttribute : Attribute
    {

    }

    public static class Publishing
    {


        public static void Publish<T>(Guid id) where T : CosmosEntity, new()
        {
            var _orm = TinyIoCContainer.Current.Resolve<ICosmosOrm>();
            var allCopies = _orm.FindContentVersions<T>(id,null).Result.ToList();

            var archiveVersion = 0m;
            var previousArchivedVersioned = allCopies.Where(x => ContentEnvironment.Archive == x.ContentEnvironment).ToList();
            if (previousArchivedVersioned.Any())
                archiveVersion = previousArchivedVersioned.Max(x => x.ContentVersion);

            foreach (var copy in allCopies)
            {
                switch (copy.ContentEnvironment)
                {
                    case ContentEnvironment.Live:
                        _orm.Delete(copy);
                        break;

                    case ContentEnvironment.Draft:
                        copy.InternalId = null;
                        copy.ContentVersion = Math.Floor(archiveVersion)+1;
                        copy.ContentEnvironment = ContentEnvironment.Live;
                        _orm.Save(copy);

                        copy.InternalId = null;
                        copy.ContentEnvironment = ContentEnvironment.Archive;
                        _orm.Save(copy);
                        break;

                    case ContentEnvironment.Archive:
                        bool isWholeVersion = copy.ContentVersion % 1 == 0;
                        if (!isWholeVersion)
                            _orm.Delete(copy);
                        break;
                }
            }

        }
    }

    public abstract class CosmosRepository<T> where T : CosmosEntity, new()
    {
        private readonly ICosmosOrm _orm;

        protected CosmosRepository():this(TinyIoCContainer.Current.Resolve<ICosmosOrm>())
        {
        }

        protected CosmosRepository(ICosmosOrm orm)
        {
            _orm = orm;
        }

        public void Save(T item) 
        {
            _orm.Save(item);
        }



        //public Task<IReadOnlyCollection<T>> Find(string condition = null) 
        //{
        //    return _orm.FindContentVersions<T>(condition);
        //}
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

        public async Task<IReadOnlyCollection<T>> FindContentVersions<T>(Guid contentId, ContentEnvironment? environment) where T : CosmosEntity, new()
        {
            //todo now: sql injection.
            var isUnversioned = typeof(T).GetCustomAttributes<UnversionedAttribute>().Any();
            if (isUnversioned || environment == null)
            {
                return await Find<T>($"ContentId eq '{contentId}'");
            }
            else
            {
                return await Find<T>($"ContentId eq '{contentId}' and PartitionKey eq '{environment}'");
            }

            
        }

        

        private async Task<IReadOnlyCollection<T>> Find<T>(string condition = null) where T : CosmosEntity, new()
        {
            AssertIsOnline();

            var cloudTable = GetOrCreateTable<T>().Result;
            var query = new TableQuery<T> { FilterString = condition };
            var items = await cloudTable.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());

            //TableOperation.Retrieve<>()
            //cloudTable.ExecuteAsync()

            //string pkSearch = $@"SELECT * FROM {table.Name}";
            //if (!string.IsNullOrEmpty(condition))
            //    pkSearch += " WHERE " + condition;
            //var items =  _client..CreateDocumentQuery<T>(_collectionUris[typeof(T)],pkSearch).ToList();

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
