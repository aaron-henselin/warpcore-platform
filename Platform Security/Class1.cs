using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarpCore.Platform.Extensibility;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace Platform_Security
{
    public interface ISupportsInheritedSecurity
    {
    }

    public class Security
    {
        protected readonly ICosmosOrm Orm;

        protected Security() : this(Dependency.Resolve<ICosmosOrm>())
        {
        }

        protected Security(ICosmosOrm orm)
        {
            Orm = orm;
        }
    }


    [Table("platform_permission_rule")]
    public class Permission : UnversionedContentEntity
    {

    }

    public class PermissionRepository : UnversionedContentRepository<Permission>
    {
    }
    
}
