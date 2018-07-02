using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WarpCore.DbEngines.AzureStorage
{
    public class PublishResult
    {
        public decimal? NewVersion { get; set; }
        public bool AlreadyInSync { get; set; }
        public Guid ContentId { get; set; }

    }

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

            SaveDraftChecksum(item);
        }

        private void SaveDraftChecksum(T item)
        {
            var contentType = typeof(T).GetCustomAttribute<TableAttribute>().Name;

            var currentPublishingData = Orm.FindUnversionedContent<ContentChecksum>("ContentId eq '" + item.ContentId + "'")
                .Result.SingleOrDefault();
            if (currentPublishingData == null)
            {
                currentPublishingData = new ContentChecksum
                {
                    ContentId = item.ContentId,
                    Draft = item.GetContentChecksum(),
                    ContentType = contentType
                };
                Orm.Save(currentPublishingData);
            }
        }


        public Task<IReadOnlyCollection<T>> FindContentVersions(string condition,
            ContentEnvironment version = ContentEnvironment.Live)
        {
            return Orm.FindContentVersions<T>(condition, version);
        }



        public IReadOnlyCollection<PublishResult> Publish(string condition)
        {
            var results = new List<PublishResult>();
            var allContentToPublish = FindContentVersions(condition, ContentEnvironment.Any).Result.ToList().ToLookup(x => x.ContentId);
            foreach (var lookupGroup in allContentToPublish)
            {
                var publishResult = Publish(lookupGroup.ToList());
                results.Add(publishResult);
            }

            return results;
        }

        private PublishResult Publish(IReadOnlyCollection<T> contentVersions)
        {
            PublishResult publishResult = new PublishResult();

            var currentLive = contentVersions.SingleOrDefault(x => ContentEnvironment.Live == x.ContentEnvironment);
            var currentDraft = contentVersions.SingleOrDefault(x => ContentEnvironment.Draft == x.ContentEnvironment);

            publishResult.ContentId = currentDraft.ContentId.Value;

            var areInSync = string.Equals(currentLive?.GetContentChecksum(),currentDraft?.GetContentChecksum());
            if (areInSync)
            {
                publishResult.AlreadyInSync = true;
                return publishResult;
            }

            var archiveVersion = 0m;
            var previousArchivedVersioned = contentVersions.Where(x => ContentEnvironment.Archive == x.ContentEnvironment).ToList();
            if (previousArchivedVersioned.Any())
                archiveVersion = previousArchivedVersioned.Max(x => x.ContentVersion);

            foreach (var copy in contentVersions)
            {
                switch (copy.ContentEnvironment)
                {
                    case ContentEnvironment.Live:
                        Orm.Delete(copy);
                        break;

                    case ContentEnvironment.Draft:
                        copy.InternalId = null;
                        copy.ContentVersion = Math.Floor(archiveVersion) + 1;
                        copy.ContentEnvironment = ContentEnvironment.Live;
                        Orm.Save(copy);

                        publishResult.NewVersion = copy.ContentVersion;

                        copy.InternalId = null;
                        copy.ContentEnvironment = ContentEnvironment.Archive;
                        Orm.Save(copy);
                        break;

                    case ContentEnvironment.Archive:
                        var isWholeVersion = copy.ContentVersion % 1 == 0;
                        if (!isWholeVersion)
                            Orm.Delete(copy);
                        break;
                }
            }

            return publishResult;

        }
    }
}