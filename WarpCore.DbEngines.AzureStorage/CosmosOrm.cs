using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
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
        Task<IReadOnlyCollection<T>> Find<T>(string condition = null) where T : CosmosEntity, new();
    }

    public abstract class CosmosEntity : TableEntity
    {
        public CosmosEntity() : base("StarfleetOps", null)
        {
        }

        [IgnoreProperty]
        public Guid? Id
        {
            get
            {
                if (RowKey == null)
                    return null;

                return new Guid(this.RowKey);
            }
            set { this.RowKey = value?.ToString(); }
        }

        private string ChangeTracking { get; set; }

        internal void InitializeChangeTracking()
        {
            ChangeTracking = JsonConvert.SerializeObject(this);
        }

        internal bool IsDirty => !string.Equals(ChangeTracking, JsonConvert.SerializeObject(this));

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

        public Task<IReadOnlyCollection<T>> Find(string condition = null) 
        {
            return _orm.Find<T>(condition);
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

        public async void Save<T>(T item) where T : CosmosEntity
        {
            AssertIsOnline();

            if (!item.IsDirty)
                return;

            if (item.Id == null)
                item.Id = Guid.NewGuid();



            var table = GetOrCreateTable<T>().Result;
            await table.ExecuteAsync(TableOperation.InsertOrReplace(item));
        }

        public async Task<IReadOnlyCollection<T>> Find<T>(string condition = null) where T : CosmosEntity, new()
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
