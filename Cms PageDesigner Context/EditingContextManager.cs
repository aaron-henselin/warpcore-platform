using Newtonsoft.Json;
using WarpCore.Cms;
using WarpCore.Platform.Kernel;

namespace Cms_PageDesigner_Context
{
    public class EditingContextManager
    {
        private readonly IHttpRequest _httpRequest;
        private readonly IHttpItems _httpItems;

        public EditingContextManager(): this(Dependency.Resolve<IHttpRequest>(),Dependency.Resolve<IHttpItems>())
        {
        }

        public EditingContextManager(IHttpRequest httpRequest, IHttpItems httpItems)
        {
            _httpRequest = httpRequest;
            _httpItems = httpItems;
        }


        private EditingContext CreateEditingContext(IHasDesignedContent hasDesignedLayout)
        {
            var ec = new EditingContext
            {
                DesignType = hasDesignedLayout.GetType().AssemblyQualifiedName,
                DesignedContentId = hasDesignedLayout.DesignForContentId,
                DesignContentTypeId = hasDesignedLayout.ContentTypeId,
                AllContent = hasDesignedLayout.DesignedContent,
            };
            var raw = JsonConvert.SerializeObject(ec);
            return JsonConvert.DeserializeObject<EditingContext>(raw);
        }

        public EditingContext GetOrCreateEditingContext(IHasDesignedContent hasDesignedLayout)
        {

            var pageDesignHasNotStarted =
                _httpRequest.QueryString[EditingContextVars.SerializedPageDesignStateKey] == null;

            if (pageDesignHasNotStarted)
                _httpItems.Set(EditingContextVars.PageDesignContextKey, CreateEditingContext(hasDesignedLayout));

            return GetEditingContext();
        }

        public EditingContext GetEditingContext()
        {
            if (_httpItems.Get<EditingContext>(EditingContextVars.PageDesignContextKey) == null)
            {
                var json = _httpRequest.QueryString[EditingContextVars.SerializedPageDesignStateKey];
                _httpItems.Set(EditingContextVars.PageDesignContextKey,JsonConvert.DeserializeObject<EditingContext>(json));
            }

            return _httpItems.Get<EditingContext>(EditingContextVars.PageDesignContextKey);
        }



    }
}