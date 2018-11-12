using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility
{
    public class ExposeToWarpCoreApi : Attribute
    {
        public ExposeToWarpCoreApi(string uid)
        {
            TypeUid = uid;
        }

        public string TypeUid { get; set; }
    }



    [Table("cms_repository_metadata")]
    public class RepositoryMetdata : UnversionedContentEntity
    {
        public bool IsDynamic { get; set; }
        public string ApiId { get; set; }
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
