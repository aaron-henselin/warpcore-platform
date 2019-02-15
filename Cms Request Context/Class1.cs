using System;

namespace Modules.Cms.Features.Context
{



    public interface ILayoutHandle
    {
        string FriendlyName { get; set; }
        string HandleName { get; set; }
        Guid PageContentId { get; set; }
    }



}
