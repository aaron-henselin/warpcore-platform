﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.Expressions;
using WarpCore.Platform.DataAnnotations.Orm;
using WarpCore.Platform.Kernel;

namespace WarpCore.Platform.Orm
{
    public class PublishResult
    {
        public decimal? NewVersion { get; set; }
        public bool AlreadyInSync { get; set; }
        public Guid ContentId { get; set; }

    }

    public interface IContentRepository
    {
    }


    public interface IUnversionedContentRepository : IContentRepository
    {
        IReadOnlyCollection<UnversionedContentEntity> FindContent(BooleanExpression condition);
    }


    public interface IVersionedContentRepository  : IContentRepository
    {
        IReadOnlyCollection<VersionedContentEntity> FindContentVersions(BooleanExpression condition,
            ContentEnvironment version = ContentEnvironment.Live);
    }

    public struct SecurityQuery
    {
        public Guid RepositoryApiId { get; set; }
        public Guid ItemId { get; set; }
    }
    public interface IRepositorySecurityModel
    {
        PermissionRuleSet CalculatePermissions(SecurityQuery securedResourceId);

    }





    public abstract class VersionedContentRepository<TVersionedContentEntity> : IVersionedContentRepository, ISupportsCmsForms where TVersionedContentEntity : VersionedContentEntity, new()
    {
        protected readonly ICosmosOrm Orm;
        private ISupportsCmsForms _supportsCmsFormsImplementation;
        protected virtual IRepositorySecurityModel SecurityModel { get; }

        protected VersionedContentRepository():this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected VersionedContentRepository(ICosmosOrm orm)
        {
            Orm = orm;
        }

        protected virtual void SaveImpl(VersionedContentEntity item)
        {
            Orm.Save(item);
            SaveDraftChecksum(item);
        }

        public void Save(TVersionedContentEntity item)
        {
            
            SaveImpl((VersionedContentEntity)item);
        }



        private void SaveDraftChecksum(VersionedContentEntity item)
        {
            var contentType = item.GetType().GetCustomAttribute<TableAttribute>().TableName;

            var booleanExpression = By.ContentId(item.ContentId);
            var filter = SqlTranslator.Build(booleanExpression, item.GetType());

            var currentPublishingData = Orm.FindUnversionedContent<ContentChecksum>(filter)
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


        IReadOnlyCollection<VersionedContentEntity> IVersionedContentRepository.FindContentVersions(BooleanExpression condition, ContentEnvironment version)
        {
            return this.FindContentVersions(condition, version).Result.Cast<VersionedContentEntity>().ToList();
            
        }

        public async Task<IReadOnlyCollection<TVersionedContentEntity>> FindContentVersions(BooleanExpression condition,
            ContentEnvironment version = ContentEnvironment.Live)
        {
            //todo: resolve <T> from metadata.
            var filter = SqlTranslator.Build(condition, typeof(TVersionedContentEntity));
            var contentItems = await Orm.FindContentVersions<TVersionedContentEntity>(filter, version);

            //if (SecurityModel != null)
            //    foreach (var contentItem in contentItems)
            //    {
            //        //todo: move attribute descriptions to extensibility
            //        var api = RepositoryExtensions.GetRepositoryAttribute(this);

            //        var permissionSet = SecurityModel.CalculatePermissions(new SecurityQuery{ItemId=contentItem.ContentId,RepositoryApiId = api.TypeUid});
            //        PermissionSetEvaluator.Assert(permissionSet, KnownPrivilegeNames.Read);
            //    }

            return contentItems;
        }



        public IReadOnlyCollection<PublishResult> Publish(BooleanExpression booleanExpression)
        {
            var results = new List<PublishResult>();
            var allContentToPublish = FindContentVersions(booleanExpression, ContentEnvironment.Any).Result.ToList().ToLookup(x => x.ContentId);
            foreach (var lookupGroup in allContentToPublish)
            {
                var publishResult = Publish(lookupGroup.ToList());
                results.Add(publishResult);
            }

            return results;
        }

        private PublishResult Publish(IReadOnlyCollection<TVersionedContentEntity> contentVersions)
        {
            PublishResult publishResult = new PublishResult();

            var currentLive = contentVersions.SingleOrDefault(x => ContentEnvironment.Live == x.ContentEnvironment);
            var currentDraft = contentVersions.SingleOrDefault(x => ContentEnvironment.Draft == x.ContentEnvironment);

            publishResult.ContentId = currentDraft.ContentId;

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


        #region ISupportsCmsForms
        WarpCoreEntity ISupportsCmsForms.New()
        {
            return new TVersionedContentEntity();
        }

        void ISupportsCmsForms.Save(WarpCoreEntity item)
        {
            this.SaveImpl((VersionedContentEntity)item);
        }

        WarpCoreEntity ISupportsCmsForms.GetById(Guid id)
        {
            return this.FindContentVersions(By.ContentId(id), ContentEnvironment.Draft).Result.Single();
        }
        #endregion

    }
}