using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorComponents.Shared
{
    public interface IUnrootedTree<T>
    {
        List<T> ChildNodes { get; }
    }

    public interface IRootedTree<T>
    {
        T RootNode { get; }
    }

    public interface ITreeNode<T> 
    {
        Guid Id { get; }
        List<T> ChildNodes { get; }
    }

    public static class TreeExtensions
    {
        public static T FindDescendentNode<T>(this IRootedTree<T> tree,Guid id) where T : ITreeNode<T>
        {
            return FindDescendentNode(tree.RootNode, id);
        }

        public static T FindDescendentNode<T>(this IUnrootedTree<T> tree, Guid id) where T : ITreeNode<T>
        {
            foreach (var child in tree.ChildNodes)
            {
                var found = FindDescendentNode(child, id);
                if (found != null)
                    return found;
            }

            return default(T);
        }

        public static T FindDescendentNode<T>(this T searchNode, Guid id) where T:ITreeNode<T>
        {
            if (searchNode.Id == id)
                return searchNode;

            foreach (var child in searchNode.ChildNodes)
            {
                var found = FindDescendentNode(child, id);
                if (found != null)
                    return found;
            }

            return default(T);
        }


        public static void RemoveDescendentNode<T>(this IRootedTree<T> tree, Guid id) where T : ITreeNode<T>
        {
            var success = RemoveDescendentNode(tree.RootNode, id);
            if (!success)
                throw new Exception("Node was not found.");


        }

        public static bool RemoveDescendentNode<T>(this T searchNode, Guid id) where T : ITreeNode<T>
        {
            var searchNodes = searchNode.ChildNodes.ToList();
            foreach (var child in searchNodes)
            {
                if (child.Id == id)
                {
                    searchNode.ChildNodes.Remove(child);
                    return true;
                }

                var found = RemoveDescendentNode(child, id);
                if (found)
                    return true;
            }

            return false;
        }
    }

    public class PageStructure : IUnrootedTree<StructureNode>
    { 
        public List<StructureNode> ChildNodes { get; set; }

    }

    public class StructureNode : ITreeNode<StructureNode>
    {
        public Guid Id { get; set; }
        public string PlacementContentPlaceHolderId { get; set; }
        public Guid? PlacementLayoutBuilderId { get; set; }
        public int Order { get; set; }
        public string WidgetTypeCode { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public List<StructureNode> ChildNodes { get; set; } = new List<StructureNode>();
    }

    public enum NodeType
    {
        Element,
        Html,
        LayoutSubtitution,
        GlobalSubstitution
    }
    public class PreviewNode : ITreeNode<PreviewNode>
    {
        public string FriendlyName { get; set; }
        public Guid ContentId { get; set; }
        public Guid Id => ContentId;

        public NodeType Type { get; set; }
        public string Html { get; set; }
        public List<PreviewNode> ChildNodes { get; set; } = new List<PreviewNode>();
        public bool IsFromLayout { get; set; }

        public Dictionary<string,string> Parameters { get; set; } = new Dictionary<string, string>();
        public string PlaceHolderId { get; set; }


    }
}
