﻿using System;
using System.Collections.Generic;
using System.Linq;
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



        public void Publish(string condition)
        {
            var allContentToPublish = FindContentVersions(condition, null).Result.ToList().ToLookup(x => x.ContentId);
            foreach (var lookupGroup in allContentToPublish)
            {
                Publish(lookupGroup.ToList());
            }

        }

        private void Publish(IReadOnlyCollection<T> contentVersions)
        {
            var currentLive = contentVersions.SingleOrDefault(x => ContentEnvironment.Live == x.ContentEnvironment);
            var currentDraft = contentVersions.SingleOrDefault(x => ContentEnvironment.Draft == x.ContentEnvironment);

            var areInSync = string.Equals(currentLive?.GetPublishingChecksum(),currentDraft?.GetPublishingChecksum());
            if (areInSync)
                return;

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

        }
    }
}