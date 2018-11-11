using System;

namespace WarpCore.Platform.Extensibility
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