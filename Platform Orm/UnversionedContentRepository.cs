using System;
using System.Collections.Generic;
using System.Linq;
using WarpCore.Platform.DataAnnotations.Expressions;
using WarpCore.Platform.Kernel;

namespace WarpCore.Platform.Orm
{



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
            var conditionText = By.ContentId(contentId);
            var filter = SqlTranslator.Build(conditionText, typeof(T));
            return Orm.FindUnversionedContent<T>(filter).Result.Single();
        }

        public IReadOnlyCollection<T> Find(BooleanExpression condition=null)
        {
            var filter = SqlTranslator.Build(condition,typeof(T));
            return Orm.FindUnversionedContent<T>(filter).Result.ToList();
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

        IReadOnlyCollection<UnversionedContentEntity> IUnversionedContentRepository.FindContent(BooleanExpression condition)
        {
            return Find(condition);
        }

    }
}