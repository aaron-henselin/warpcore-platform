using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using WarpCore.Data.Schema;
using WarpCore.Kernel;

namespace WarpCore.DbEngine.Azure
{
    public class AzureClientFactory
    {
        private readonly CloudStorageAccount _storageAccount;

        public AzureClientFactory(WarpCoreDataAccessConfig dataAccessConfig) : this(dataAccessConfig.StorageConnectionString)
        {

        }

        public AzureClientFactory(string connectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(connectionString);

        }

        public CloudTableClient CreateCloudTableClient()
        {
            return _storageAccount.CreateCloudTableClient();
        }
    }

    public class AzureDataAccessException :Exception
    {
        public AzureDataAccessException(string message) : base(message)
        {
        }
    }
}
