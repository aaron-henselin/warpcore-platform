using System;
using System.Collections.Generic;

namespace WarpCore.Platform.Orm
{
    public interface IContentRepository
    {
        WarpCoreEntity New();

        void Save(WarpCoreEntity item);
    }

    public static class IContentRepositoryExtensions
    {
        public static IReadOnlyCollection<WarpCoreEntity> FindContent(this IContentRepository contentRepository, string filter, ContentEnvironment version = ContentEnvironment.Live)
        {
            if (contentRepository is IVersionedContentRepository @base)
                return @base.FindContentVersions(filter, version);

            if (contentRepository is IUnversionedContentRepository repositoryBase)
                return repositoryBase.FindContent(filter);

            throw new Exception();
        }
    }
}