using System;
using System.Collections.Generic;

namespace BlazorComponents.Shared
{
    public class SitemapModel : IUnrootedTree<SitemapModelNode>
    {
        public static Guid RootId { get; } = new Guid("00000000-0000-0000-0000-000000000001");
        public List<SitemapModelNode> ChildNodes { get; set; } = new List<SitemapModelNode>();
        public string SiteName { get; set; }
        public Guid SiteId { get; set; }
        public bool IsFrontendSite { get; set; }
    }

    public class SitemapModelNode : ITreeNode<SitemapModelNode>
    {
        public Guid Id { get; set; }
        public List<SitemapModelNode> ChildNodes { get; set; } = new List<SitemapModelNode>();
        public string Name { get; set; }
        public Guid PageId { get; set; }
        public string VirtualPath { get; set; }
        public int Depth { get; set; }
        public string ParentPath { get; set; }
        public bool HasChildItems { get; set; }
        public bool IsPublished { get; set; }
        public bool IsHomePage { get; set; }
        public string DesignUrl { get; set; }
        public string SettingsUrl { get; set; }
        public bool IsExpanded { get; set; }
    }
    
}
