using System;
using System.Linq;

namespace WarpCore.DbEngines.AzureStorage
{
    public static class Publishing
    {


        public static void Publish<T>(Guid id) where T : CosmosEntity, new()
        {
            var _orm =Dependency.Resolve<ICosmosOrm>();
            var allCopies = _orm.FindContentVersions<T>(id,null).Result.ToList();

            var archiveVersion = 0m;
            var previousArchivedVersioned = allCopies.Where(x => ContentEnvironment.Archive == x.ContentEnvironment).ToList();
            if (previousArchivedVersioned.Any())
                archiveVersion = previousArchivedVersioned.Max(x => x.ContentVersion);

            foreach (var copy in allCopies)
            {
                switch (copy.ContentEnvironment)
                {
                    case ContentEnvironment.Live:
                        _orm.Delete(copy);
                        break;

                    case ContentEnvironment.Draft:
                        copy.InternalId = null;
                        copy.ContentVersion = Math.Floor(archiveVersion)+1;
                        copy.ContentEnvironment = ContentEnvironment.Live;
                        _orm.Save(copy);

                        copy.InternalId = null;
                        copy.ContentEnvironment = ContentEnvironment.Archive;
                        _orm.Save(copy);
                        break;

                    case ContentEnvironment.Archive:
                        var isWholeVersion = copy.ContentVersion % 1 == 0;
                        if (!isWholeVersion)
                            _orm.Delete(copy);
                        break;
                }
            }

        }
    }
}