using System;

namespace WarpCore.Platform.DataAnnotations
{
    public class DataRelationAttribute : Attribute
    {
        public string ApiId { get; set; }

        public DataRelationAttribute(string apiId)
        {
            ApiId = apiId;
        }


    }
}