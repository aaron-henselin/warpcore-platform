using System;
using System.Diagnostics;
using System.Linq;
using WarpCore.Platform.DataAnnotations;
using WarpCore.Platform.DataAnnotations.Orm;
using WarpCore.Platform.Orm;

namespace WarpCore.Platform.Extensibility
{
    /// <summary>
    /// Exposes the repository to the WarpCore Api. This allows users to create forms, workflows, and custom fields for this type from the frontend.
    /// </summary>
    [DebuggerDisplay("Uid = {TypeUid}")]
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


    [DebuggerDisplay("ApiId = {ApiId},TypeName = {GetTypeNameDebuggerDisplay()}")]
    [Table("cms_repository_metadata")]
    [WarpCoreEntity("204b8498-b8b7-489e-87ed-ddb1fcfb6608",SupportsCustomFields = false)]
    public class RepositoryMetdata : UnversionedContentEntity
    {
        public bool IsDynamic { get; set; }
        public Guid ApiId { get; set; }
        public string AssemblyQualifiedTypeName { get; set; }
        public string CustomAssemblyQualifiedTypeName { get; set; }
        public string CustomRepositoryName { get; set; }

        private string GetTypeNameDebuggerDisplay()
        {
            return CustomAssemblyQualifiedTypeName ?? AssemblyQualifiedTypeName;
        }
    }



    [ExposeToWarpCoreApi(ApiId)]
    public class RepositoryMetadataManager : UnversionedContentRepository<RepositoryMetdata>
    {
        public const string ApiId = "3a9a6f79-9564-4b51-af1c-9d926fddbc35";
        public RepositoryMetdata GetRepositoryMetdataByTypeResolverUid(Guid formInteropUid)
        {
            var text = nameof(RepositoryMetdata.ApiId) + " == {" + formInteropUid + "}";

            return Find(By.Condition(text)).First();
        }
    }

}
