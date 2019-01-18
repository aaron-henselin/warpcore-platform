using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorComponents.Shared
{

    public enum NodeType
    {
        Element,
        Html,
        LayoutSubtitution,
        GlobalSubstitution
    }

    public class PageStructure
    {
        public List<StructureNode> Nodes { get; set; }
    }

    public class StructureNode
    {
        public Guid Id { get; set; }
        public string PlacementContentPlaceHolderId { get; set; }
        public Guid? PlacementLayoutBuilderId { get; set; }
        public int Order { get; set; }
        public string WidgetTypeCode { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public List<StructureNode> AllContent { get; set; } = new List<StructureNode>();
    }

    public class PreviewNode
    {
        public string FriendlyName { get; set; }
        public Guid ContentId { get; set; }
        public NodeType Type { get; set; }
        public string Html { get; set; }
        public List<PreviewNode> ChildNodes { get; set; } = new List<PreviewNode>();
    }
}
