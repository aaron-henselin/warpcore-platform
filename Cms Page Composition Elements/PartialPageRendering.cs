using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WarpCore.Cms;

namespace Modules.Cms.Features.Presentation.PageComposition.Elements
{
    public class InternalLayout
    {
        public List<string> PlaceHolderIds { get; } = new List<string>();
        public List<CmsPageContent> DefaultContent { get; } = new List<CmsPageContent>();

        public static InternalLayout Empty => new InternalLayout();
    }

    public interface IHasInternalLayout
    {
        //IReadOnlyCollection<CmsPageContent> GetDefaultPlaceHolderContents(ILayoutControl layoutControl,
        //    string placeholderId);

        InternalLayout GetInternalLayout();

        //protected void ActivateLayout(ILayoutControl layout)
        //{
        //    if (layout == null)
        //        return;

        //    var generatedPlaceHolders = layout.InitializeLayout();
        //    //foreach (var ph in generatedPlaceHolders)
        //    //{

        //    //    //return _cmsForm.DesignedContent;
        //    //    //var contentActivator = new CmsPageContentActivator();
        //    //    //return _cmsForm.DesignedContent.Select(x => contentActivator.ActivateCmsPageContent(x)).ToList();

        //    //    this.PlaceHolders.Add(ph, new RenderingsPlaceHolder { Id = ph, Renderings = layout.GetAutoIncludedElementsForPlaceHolder(ph).Select(factory).ToList() });
        //    //}

        //    this.LayoutBuilderId = layout.LayoutBuilderId;
        //}
    }

    [DebuggerDisplay("Name = {"+nameof(FriendlyName)+"}, ContentId = {" + nameof(ContentId)+"}" )]
    public class PageCompositionElement 
    {
        public Guid ContentId { get; set; }
        public string LocalId { get; set; }
        public Guid LayoutBuilderId { get; set; }
        public bool IsFromLayout { get; set; }
        public string FriendlyName { get; set; }
        public Dictionary<string, RenderingsPlaceHolder> PlaceHolders { get; } = new Dictionary<string, RenderingsPlaceHolder>();
        public List<string> GlobalPlaceHolders { get; } = new List<string>();
        public string CacheKey { get; set; }

        public int SelfAndDescendentElementCount => GetAllDescendents().Count;

        public IReadOnlyCollection<PageCompositionElement> GetAllDescendents()
        {
            return GetAllDescendents(this);
        }

        private IReadOnlyCollection<PageCompositionElement> GetAllDescendents(PageCompositionElement parent)
        {
            List<PageCompositionElement> partials = new List<PageCompositionElement>();

            partials.Add(parent);

            foreach (var ph in parent.PlaceHolders.Values)
            foreach (var child in ph.Renderings)
            {
                var desc = GetAllDescendents(child);
                partials.AddRange(desc);
            }

            return partials;
        }
    }
}