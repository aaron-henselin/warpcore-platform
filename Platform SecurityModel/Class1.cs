using System;

using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.Kernel;
using WarpCore.Platform.Orm;

namespace Platform_Security
{



    [Table("platform_permission_rule")]
    public class PermissionRule : UnversionedContentEntity, IPermissionRule
    {
        public Guid SecuredResourceId { get; set; }
        public string AppliesToRoleName { get; set; }
        public PermissionType PermissionType { get; set; }
        public string PrivilegeName { get; set; }
        public string SecurityModel { get; set; }
    }

    public class PermissionRepository : UnversionedContentRepository<PermissionRule>
    {
    }
    
}
