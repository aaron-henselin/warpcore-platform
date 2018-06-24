using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarpCore.DbEngines.AzureStorage
{
    public abstract class UnversionedCosmosRepository<T> where T : CosmosEntity, new()
    {
        protected readonly ICosmosOrm _orm;

        protected UnversionedCosmosRepository() : this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected UnversionedCosmosRepository(ICosmosOrm orm)
        {
            _orm = orm;
        }

        public virtual void Save(T item)
        {
            _orm.Save(item);
        }

        public T GetById(Guid contentId)
        {
            return _orm.FindContentVersions<T>(contentId, ContentEnvironment.Unversioned).Result.Single();
        }

        public IReadOnlyCollection<T> Find(string condition=null)
        {
            return _orm.FindContentVersions<T>(condition, ContentEnvironment.Unversioned).Result.ToList();
        }

        
    }

    public abstract class VersionedCosmosRepository<T> where T : CosmosEntity, new()
    {
        protected readonly ICosmosOrm _orm;

        protected VersionedCosmosRepository():this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected VersionedCosmosRepository(ICosmosOrm orm)
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