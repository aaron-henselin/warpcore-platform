using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarpCore.Platform.Kernel;

namespace WarpCore.Platform.Orm
{
    public interface ICosmosOrm
    {
        void Save(WarpCoreEntity item);

        Task<IReadOnlyCollection<T>> FindContentVersions<T>(string condition,
            ContentEnvironment version = ContentEnvironment.Live)
            where T : VersionedContentEntity, new();

        Task<IReadOnlyCollection<T>> FindUnversionedContent<T>(string condition)
            where T : UnversionedContentEntity, new();

        void Delete(WarpCoreEntity copy);
    }

    public class SerializedComplexObjectAttribute : Attribute
    {
    }

    public abstract class UnversionedContentRepository<T> : IUnversionedContentRepository where T : UnversionedContentEntity, new()
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

        IReadOnlyCollection<UnversionedContentEntity> IUnversionedContentRepository.FindContent(string condition)
        {
            return Find(condition);
        }
    }
}