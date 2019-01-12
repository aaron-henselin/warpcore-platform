using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Platform.Kernel;

namespace WarpCore.Platform.Orm
{


    public class SerializedComplexObjectAttribute : Attribute
    {
    }

    public abstract class UnversionedContentRepository<T> : IUnversionedContentRepository, ISupportsCmsForms where T : UnversionedContentEntity, new()
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

        WarpCoreEntity ISupportsCmsForms.New()
        {
            return new T();
        }

        void ISupportsCmsForms.Save(WarpCoreEntity item)
        {
            Save((T)item);
        }

        WarpCoreEntity ISupportsCmsForms.GetById(Guid id)
        {
            return this.GetById(id);
        }

        IReadOnlyCollection<UnversionedContentEntity> IUnversionedContentRepository.FindContent(string condition)
        {
            return Find(condition);
        }

    }
}