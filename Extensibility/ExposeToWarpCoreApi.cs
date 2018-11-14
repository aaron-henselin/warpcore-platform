using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility
{
    /// <summary>
    /// Exposes the repository to the WarpCore Api. This allows users to create forms, workflows, and custom fields for this type from the frontend.
    /// </summary>
    public class ExposeToWarpCoreApi : Attribute
    {
        public ExposeToWarpCoreApi(string uid) : this(new Guid(uid))
        {
        }

        public ExposeToWarpCoreApi(Guid uid)
        {
            TypeUid = uid;
        }

        public Guid TypeUid { get; set; }
    }



    [Table("cms_repository_metadata")]
    public class RepositoryMetdata : UnversionedContentEntity
    {
        public bool IsDynamic { get; set; }
        public Guid ApiId { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }

        public string CustomAssemblyQualifiedTypeName { get; set; }
        public string CustomRepositoryName { get; set; }
    }



    [ExposeToWarpCoreApi(ApiId)]
    public class RepositoryMetadataManager : UnversionedContentRepository<RepositoryMetdata>
    {
        public const string ApiId = "3a9a6f79-9564-4b51-af1c-9d926fddbc35";
        public RepositoryMetdata GetRepositoryMetdataByTypeResolverUid(Guid formInteropUid)
        {
            return Find(nameof(RepositoryMetdata.ApiId) + " eq '" + formInteropUid + "'").First();
        }
    }

}
