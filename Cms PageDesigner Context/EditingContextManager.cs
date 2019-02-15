using Newtonsoft.Json;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Cms_PageDesigner_Context
{
    //public class EditingContextManager
    //{
    //    private readonly IHttpRequest _httpRequest;
    //    private readonly IPerRequestItems _perRequestItems;

    //    public EditingContextManager(): this(Dependency.Resolve<IHttpRequest>(),Dependency.Resolve<IPerRequestItems>())
    //    {
    //    }

    //    public EditingContextManager(IHttpRequest httpRequest, IPerRequestItems perRequestItems)
    //    {
    //        _httpRequest = httpRequest;
    //        _perRequestItems = perRequestItems;
    //    }


    //    private EditingContext CreateEditingContext(IHasDesignedContent hasDesignedLayout)
    //    {
    //        var ec = new EditingContext
    //        {
    //            DesignType = hasDesignedLayout.GetType().AssemblyQualifiedName,
    //            DesignedContentId = hasDesignedLayout.DesignForContentId,
    //            DesignContentTypeId = hasDesignedLayout.ContentTypeId,
    //            AllContent = hasDesignedLayout.ChildNodes,
    //        };
    //        var raw = JsonConvert.SerializeObject(ec);
    //        return JsonConvert.DeserializeObject<EditingContext>(raw);
    //    }

    //    public EditingContext GetOrCreateEditingContext(IHasDesignedContent hasDesignedLayout)
    //    {

    //        var pageDesignHasNotStarted =
    //            _httpRequest.QueryString[EditingContextVars.SerializedPageDesignStateKey] == null;

    //        if (pageDesignHasNotStarted)
    //            _perRequestItems.Set(EditingContextVars.PageDesignContextKey, CreateEditingContext(hasDesignedLayout));

    //        return GetEditingContext();
    //    }

    //    public EditingContext GetEditingContext()
    //    {
    //        if (_perRequestItems.Get<EditingContext>(EditingContextVars.PageDesignContextKey) == null)
    //        {
    //            var json = _httpRequest.QueryString[EditingContextVars.SerializedPageDesignStateKey];
    //            _perRequestItems.Set(EditingContextVars.PageDesignContextKey,JsonConvert.DeserializeObject<EditingContext>(json));
    //        }

    //        return _perRequestItems.Get<EditingContext>(EditingContextVars.PageDesignContextKey);
    //    }



    //}
}