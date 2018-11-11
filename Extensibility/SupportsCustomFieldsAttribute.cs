using System;

namespace WarpCore.Cms
{
    public class SupportsCustomFieldsAttribute : Attribute
    {
        public SupportsCustomFieldsAttribute(string typeExtensionId)
        {
            TypeExtensionUid = new Guid(typeExtensionId);
        }

        public Guid TypeExtensionUid { get; set; }
    }
}