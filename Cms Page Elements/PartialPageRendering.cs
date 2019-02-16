using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Modules.Cms.Features.Presentation.Page.Elements
{
    public class PageLayout
    {
        public string Name { get; set; }
        public string MasterPagePath { get; set; }
        public List<PageContent> AllContent { get; set; } = new List<PageContent>();
        public PageLayout ParentLayout { get; set; }
    }

    public class PageContent
    {
        public Guid Id { get; set; }
        public string PlacementContentPlaceHolderId { get; set; }
        public Guid? PlacementLayoutBuilderId { get; set; }
        public int Order { get; set; }
        public string WidgetTypeCode { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public List<PageContent> AllContent { get; set; } = new List<PageContent>();

    }

    public class InternalLayout
    {
        public List<string> PlaceHolderIds { get; } = new List<string>();
        public List<PageContent> DefaultContent { get; } = new List<PageContent>();

        public static InternalLayout Empty => new InternalLayout();
    }



    public interface IHasInternalLayout
    {
      
        InternalLayout GetInternalLayout();
        
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