using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool IsFromLayout { get; set; }

        public Dictionary<string,string> Parameters { get; set; }
        public string PlaceHolderId { get; set; }

        public Node FindDescendentNode(Guid id)
        {
            return FindDescendentNode(this, id);
        }

        private Node FindDescendentNode(Node searchNode, Guid id)
        {
            if (searchNode.ContentId == id)
                return this;

            foreach (var child in searchNode.ChildNodes)
            {
                var found = FindDescendentNode(child,id);
                if (found != null)
                    return found;
            }

            return null;
        }

        public void RemoveDescendentNode(Guid id)
        {
            var success = RemoveDescendentNode(this, id);
            if (!success)
                throw new Exception("Node was not found.");


        }

        private bool RemoveDescendentNode(Node searchNode, Guid id)
        {
            var searchNodes = this.ChildNodes.ToList();
            foreach (var child in searchNodes)
            {
                if (child.ContentId == id)
                {
                    this.ChildNodes.Remove(child);
                    return true;
                }

                var found = RemoveDescendentNode(child, id);
                if (found)
                    return true;
            }

            return false;
        }
    }
}
