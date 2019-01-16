using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using BlazorComponents.Shared;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms;
using WarpCore.Cms.Sites;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Orm;
using WarpCore.Web.Widgets.FormBuilder.Support;

namespace BackendSiteApi
{
    public class PageDesignerApiController : ApiController
    {
        [HttpGet]
        [Route("api/design/page")]
        public Node Page(Guid pageId)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();
            var page = new PageCompositionBuilder().CreatePageComposition(draft,PageRenderMode.PageDesigner);

            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page, FragmentRenderMode.PageDesigner);
            var compositor = new RenderFragmentCompositor(page, batch);

            var treeWriter= new TreeHtmlWriter(draft);

            compositor.WriteComposedFragments(FragmentRenderMode.PageDesigner,treeWriter);

            return treeWriter.RootNode;
        }

        [HttpGet]
        [Route("api/design/toolbox")]
        public ToolboxViewModel Toolbox()
        {
            var vm = new ToolboxViewModel();
            
            var allItems = new ToolboxManager().Find();
            var allCategories = allItems.Select(x => x.Category).Distinct();
            foreach (var category in allCategories)
                vm.ToolboxCategories.Add(new ToolboxCategory
                {
                    CategoryName = category,
                    Items = allItems.Where(x => x.Category == category)
                    .Select(x => new ToolboxItemViewModel
                    {
                        Name = x.FriendlyName,
                        Description= x.Description,
                        WidgetTypeCode = x.WidgetUid
                    }).ToList()
                });

            return vm;
        }
    }




    public class TreeHtmlWriter : ComposedHtmlWriter
    {
        private readonly CmsPage _draft;

        public TreeHtmlWriter(CmsPage draft)
        {
            _draft = draft;
        }

        private Stack<Node> Node = new Stack<Node>();

        public Node RootNode { get; set; }

        public void BeginWriting(CompostedContentMetdata metadata)
        {
            var nodeToWrite = new Node();
            if (Node.Count > 0)
                CurrentNode.ChildNodes.Add(nodeToWrite);
            else
                RootNode = nodeToWrite;
            
            nodeToWrite.ContentId = metadata.ContentId;
            nodeToWrite.PlaceHolderId = metadata.PlaceHolderId;

            if (metadata.NodeType == FragmentType.Html)
                nodeToWrite.Type = NodeType.Html;

            if (metadata.NodeType == FragmentType.Element)
                nodeToWrite.Type = NodeType.Element;

            if (metadata.NodeType == FragmentType.LayoutSubtitution)
                nodeToWrite.Type = NodeType.LayoutSubtitution;

            if (metadata.NodeType == FragmentType.GlobalSubstitution)
                nodeToWrite.Type = NodeType.GlobalSubstitution;

            Node.Push(nodeToWrite);

            WriteMetadata();
        }


        private Node CurrentNode => Node.Peek();

        private void WriteMetadata()
        {
            CmsPageContent contentPlacement = null;
            if (CurrentNode.Type == NodeType.Element)
                contentPlacement = _draft.FindContentById(CurrentNode.ContentId);

            if (contentPlacement == null)
                CurrentNode.IsFromLayout = true;
            else
            {
                CurrentNode.Parameters = contentPlacement.Parameters;
                var toolboxItem = new ToolboxManager().GetToolboxItemByCode(contentPlacement.WidgetTypeCode);
                CurrentNode.FriendlyName = toolboxItem.FriendlyName;
            }
        }


        public void EndWriting()
        {
            Node.Pop();
        }

        public void Write(string html)
        {
            CurrentNode.ChildNodes.Add(new Node {Type=NodeType.Html,Html=html });
        }
    }

}
