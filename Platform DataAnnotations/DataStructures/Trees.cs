using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.DataAnnotations
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
        public static IReadOnlyCollection<T> GetAllDescendents<T>(this IUnrootedTree<T> tree) where T : ITreeNode<T>
        {
            List<T> foundNodes = new List<T>();
            foreach (var childNode in tree.ChildNodes)
                foundNodes.AddRange(childNode.GetAllDescendentsAndSelf());

            return foundNodes;
        }

        public static IReadOnlyCollection<T> GetAllDescendentsAndSelf<T>(this T treeNode) where T : ITreeNode<T>
        {
            List<T> foundNodes = new List<T>();
            foundNodes.Add(treeNode);
            foreach (var childNode in treeNode.ChildNodes)
                foundNodes.AddRange(childNode.GetAllDescendentsAndSelf());
            return foundNodes;
        }

        public static T FindDescendentNode<T>(this IUnrootedTree<T> tree, Guid id) where T : ITreeNode<T>
        {
            return FindDescendentNode(tree, id, out _);
        }

        public static T FindDescendentNode<T>(this IRootedTree<T> tree, Guid id) where T : ITreeNode<T>
        {
            return FindDescendentNode(tree, id, out _);
        }


        public static T FindDescendentNode<T>(this IRootedTree<T> tree, Guid id, out IReadOnlyCollection<T> searchPath) where T : ITreeNode<T>
        {
            var searchStack = new Stack<T>();
            var found = FindDescendentNode(tree.RootNode, id, searchStack);
            searchPath = searchStack.ToArray();
            return found;
        }


        public static T FindDescendentNode<T>(this IUnrootedTree<T> tree, Guid id, out IReadOnlyCollection<T> searchPath) where T : ITreeNode<T>
        {
            var searchStack = new Stack<T>();
            foreach (var child in tree.ChildNodes)
            {
                var found = FindDescendentNode(child, id, searchStack);
                if (found != null)
                {
                    searchPath = searchStack.ToArray();
                    return found;
                }
            }

            searchPath = new List<T>();
            return default(T);
        }

        public static T FindDescendentNode<T>(this T searchNode, Guid id, Stack<T> currentSearchPath) where T : ITreeNode<T>
        {
            currentSearchPath.Push(searchNode);

            if (searchNode.Id == id)
                return searchNode;

            foreach (var child in searchNode.ChildNodes)
            {
                var found = FindDescendentNode(child, id, currentSearchPath);
                if (found != null)
                    return found;
            }

            currentSearchPath.Pop();
            return default(T);
        }


        public static void RemoveDescendentNode<T>(this IUnrootedTree<T> tree, Guid id) where T : ITreeNode<T>
        {
            foreach (var child in tree.ChildNodes)
            {
                var success = RemoveDescendentNode(child, id);
                if (success)
                    return;
            }

            throw new Exception("Node was not found.");

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


}
