using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarpCore.DbEngines.AzureStorage
{
    public abstract class CosmosRepository<T> where T : CosmosEntity, new()
    {
        protected readonly ICosmosOrm _orm;

        protected CosmosRepository():this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected CosmosRepository(ICosmosOrm orm)
        {
            _orm = orm;
        }

        public virtual void Save(T item) 
        {
            _orm.Save(item);
        }

        public Task<IReadOnlyCollection<T>> FindContentVersions(Guid contentId,
            ContentEnvironment? version = ContentEnvironment.Live)
        {
            return _orm.FindContentVersions<T>(contentId, version);
        }

        public Task<IReadOnlyCollection<T>> FindContentVersions(string condition,
            ContentEnvironment? version = ContentEnvironment.Live)
        {
            return _orm.FindContentVersions<T>(condition, version);
        }



        //public Task<IReadOnlyCollection<T>> Find(string condition = null) 
        //{
        //    return _orm.FindContentVersions<T>(condition);
        //}
    }
}