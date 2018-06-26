using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarpCore.DbEngines.AzureStorage
{
    public abstract class VersionedContentRepository<T> where T : VersionedContentEntity, new()
    {
        protected readonly ICosmosOrm Orm;

        protected VersionedContentRepository():this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected VersionedContentRepository(ICosmosOrm orm)
        {
            Orm = orm;
        }

        public virtual void Save(T item) 
        {
            Orm.Save(item);
        }


        public Task<IReadOnlyCollection<T>> FindContentVersions(string condition,
            ContentEnvironment? version = ContentEnvironment.Live)
        {
            return Orm.FindContentVersions<T>(condition, version);
        }



        //public Task<IReadOnlyCollection<T>> Find(string condition = null) 
        //{
        //    return _orm.FindContentVersions<T>(condition);
        //}
    }
}