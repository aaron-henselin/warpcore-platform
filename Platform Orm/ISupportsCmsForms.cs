using System;
using System.Collections.Generic;

namespace WarpCore.Platform.Orm
{
    /// <summary>
    /// Implementing this interface allows the user to interact with a given repository through Cms Forms.
    /// </summary>
    public interface ISupportsCmsForms
    {
        WarpCoreEntity New();
        void Save(WarpCoreEntity item);
        WarpCoreEntity GetById(Guid id);
    }


    //public static class IContentRepositoryExtensions
    //{
    //    public static IReadOnlyCollection<WarpCoreEntity> FindContent(this ISupportsCmsForms contentRepository, string filter, ContentEnvironment version = ContentEnvironment.Live)
    //    {
    //        if (contentRepository is IVersionedContentRepository @base)
    //            return @base.FindContentVersions(filter, version);

    //        if (contentRepository is IUnversionedContentRepository repositoryBase)
    //            return repositoryBase.FindContent(filter);

    //        throw new Exception();
    //    }
    //}
}