using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WarpCore.Platform.DataAnnotations;

namespace BlazorComponents.Shared
{


    public class FormSessionParameters
    {
        public Guid FormId { get; set; }
        public Guid? ContentId { get; set; }
    }

    public class EditingSession
    {
        public IDictionary<string,string> InitialValues { get; set; }
        public Dictionary<string, LocalDataSource> LocalDataSources { get; set; } = new Dictionary<string, LocalDataSource>();
        
    }



    public class EntityMetadata
    {
        public string TitleProperty { get; set; }
    }


    public class LocalDataSource
    {
        public List<DataSourceItem> Items { get; set; } = new List<DataSourceItem>();

        public DataSourceItem GetItemByValue(string @value)
        {
            return Items.SingleOrDefault(x => x.Value == @value);
        }
    }

    //public class DataSourceItem
    //{
    //    public string Name { get; set; }
    //    public string Value { get; set; }
    //}

    public class ConfiguratorFormDescription
    {
        public PageStructure Layout { get; set; }
        public Dictionary<string,string> DefaultValues { get; set; }
        public EntityMetadata Metadata { get; set; }
    }

    public class PageStructure : IUnrootedTree<StructureNode>
    {
        public static Guid RootId { get; } = new Guid("00000000-0000-0000-0000-000000000001");
        public List<StructureNode> ChildNodes { get; set; } = new List<StructureNode>();

        public StructureNode GetStructureNodeById(Guid id)
        {
            var relatedPageContent = this.FindDescendentNode(id);
            if (relatedPageContent == null)
                throw new Exception("Node with id " + id + " could not be found in the current page structure.");

            return relatedPageContent;
        }

        public void Add(StructureNode newStructureNode, Guid parentNodeId)
        {
            if (PageStructure.RootId == parentNodeId)
            {
                ChildNodes.Add(newStructureNode);
            }
            else
            {
                var addToStructureNode = this.FindDescendentNode(parentNodeId);
                if (addToStructureNode == null)
                    throw new Exception("A structure node of " + parentNodeId + " was not found in the current page structure.");

                addToStructureNode.ChildNodes.Add(newStructureNode);
            }
        }
    }

    public class StructureNode : ITreeNode<StructureNode>
    {
        public Guid Id { get; set; }
        public string PlacementContentPlaceHolderId { get; set; }
        public Guid? PlacementLayoutBuilderId { get; set; }
        public int Order { get; set; }
        public string WidgetTypeCode { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
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
        public Guid Id => PreviewNodeId;

        public Guid PreviewNodeId { get; set; }

        public NodeType Type { get; set; }
        public string Html { get; set; }
        public List<PreviewNode> ChildNodes { get; set; } = new List<PreviewNode>();
        public bool IsFromLayout { get; set; }

        public Dictionary<string,string> Parameters { get; set; } = new Dictionary<string, string>();
        public string PlaceHolderId { get; set; }
        public bool UseClientRenderer { get; set; }
    }

    public class LayoutPosition
    {
        public Guid ContentId { get; set; }
        public string PlaceHolderId { get; set; }
    }
}
