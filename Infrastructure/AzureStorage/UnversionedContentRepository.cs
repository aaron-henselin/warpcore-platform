using System;
using System.Collections.Generic;
using System.Linq;

namespace WarpCore.DbEngines.AzureStorage
{
    public abstract class UnversionedContentRepository<T> : IUnversionedContentRepositoryBase where T : UnversionedContentEntity, new()
    {
        protected readonly ICosmosOrm Orm;

        protected UnversionedContentRepository() : this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected UnversionedContentRepository(ICosmosOrm orm)
        {
            Orm = orm;
        }

        public virtual void Save(T item)
        {
            Orm.Save(item);
        }

        public T GetById(Guid contentId)
        {
            return Orm.FindUnversionedContent<T>(By.ContentId(contentId)).Result.Single();
        }

        public IReadOnlyCollection<T> Find(string condition=null)
        {
            return Orm.FindUnversionedContent<T>(condition).Result.ToList();
        }

        WarpCoreEntity IContentRepository.New()
        {
            return new T();
        }

        void IContentRepository.Save(WarpCoreEntity item)
        {
            Save((T)item);
        }

        IReadOnlyCollection<UnversionedContentEntity> IUnversionedContentRepositoryBase.FindContent(string condition)
        {
            return Find(condition);
        }
    }
}