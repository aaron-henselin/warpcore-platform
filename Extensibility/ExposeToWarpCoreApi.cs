using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.DbEngines.AzureStorage;

namespace Extensibility
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



    public class RepositoryMetadataManager : UnversionedContentRepository<RepositoryMetdata>
    {
        public RepositoryMetdata GetRepositoryMetdataByTypeResolverUid(Guid formInteropUid)
        {
            return Find(nameof(RepositoryMetdata.ApiId) + " eq '" + formInteropUid + "'").First();
        }
    }

}
