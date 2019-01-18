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
    public class Node
    {
        public string FriendlyName { get; set; }
        public Guid ContentId { get; set; }
        public NodeType Type { get; set; }
        public string Html { get; set; }
        public List<Node> ChildNodes { get; set; } = new List<Node>();
    }
}
