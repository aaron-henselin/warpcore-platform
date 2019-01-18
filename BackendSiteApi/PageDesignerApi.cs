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
using WarpCore.Platform.Orm;

namespace BackendSiteApi
{
    public class PageDesignerApiController : ApiController
    {
        [HttpGet]
        [Route("api/design/page/{pageId}/preview")]
        public PreviewNode Page(Guid pageId)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();
            var page = new PageCompositionBuilder().CreatePageComposition(draft,PageRenderMode.PageDesigner);
            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page, FragmentRenderMode.PageDesigner);
            var compositor = new RenderFragmentCompositor(page, batch);
            var treeWriter= new PagePreviewWriter();

            compositor.WriteComposedFragments(FragmentRenderMode.PageDesigner,treeWriter);
            return treeWriter.RootNode;
        }

        [HttpPost]
        [Route("api/design/page/{pageId}/preview")]
        public PreviewNode Page(Guid pageId, PageStructure pageStructure)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();

            new StructureNodeConverter().ApplyNewStructureToCmsPage(draft, pageStructure);

            var page = new PageCompositionBuilder().CreatePageComposition(draft, PageRenderMode.PageDesigner);

            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page, FragmentRenderMode.PageDesigner);
            var compositor = new RenderFragmentCompositor(page, batch);
            var treeWriter = new PagePreviewWriter();

            compositor.WriteComposedFragments(FragmentRenderMode.PageDesigner, treeWriter);
            return treeWriter.RootNode;
        }

        [HttpGet]
        [Route("api/design/page/{pageId}/structure")]
        public PageStructure PageStructure(Guid pageId)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();
            return new StructureNodeConverter().GetPageStructure(draft);
        }

      
    }

    public class StructureNodeConverter
    {
        public void ApplyNewStructureToCmsPage(CmsPage draft, PageStructure pageStructure)
        {
            draft.PageContent = pageStructure.Nodes.Select(ApplyNewStructure).ToList();
        }

        public CmsPageContent ApplyNewStructure(StructureNode node)
        {
            return new CmsPageContent
            {
                Id = node.Id,
                Order = node.Order,
                PlacementContentPlaceHolderId = node.PlacementContentPlaceHolderId,
                PlacementLayoutBuilderId = node.PlacementLayoutBuilderId,
                Parameters = node.Parameters,
                WidgetTypeCode = node.WidgetTypeCode,
                AllContent = node.AllContent.Select(ApplyNewStructure).ToList()
            };
        }

        public PageStructure GetPageStructure(CmsPage draft)
        {
            return new PageStructure
            {
                Nodes = draft.PageContent.Select(GetPageStructure).ToList()
            };
        }

        private StructureNode GetPageStructure(CmsPageContent content)
        {
            var sn = new StructureNode
            {
                Id = content.Id,
                Order = content.Order,
                Parameters = content.Parameters,
                PlacementContentPlaceHolderId = content.PlacementContentPlaceHolderId,
                PlacementLayoutBuilderId = content.PlacementLayoutBuilderId,
                WidgetTypeCode = content.WidgetTypeCode,
                AllContent = content.AllContent.Select(GetPageStructure).ToList()
            };
            return sn;
        }
    }


    public class PagePreviewWriter : ComposedHtmlWriter
    {
        public PreviewNode RootNode { get; set; }

        public PreviewNode ParentNode { get; set; }
        public PreviewNode CurrentNode { get; set; }

        public void BeginWriting(CompostedContentMetdata metadata)
        {
            var nodeToWrite = new PreviewNode();
            nodeToWrite.ContentId = metadata.ContentId;
            nodeToWrite.FriendlyName = metadata.FriendlyName;

            if (metadata.NodeType == FragmentType.Element)
                nodeToWrite.Type = NodeType.Element;

            if (metadata.NodeType == FragmentType.LayoutSubtitution)
                nodeToWrite.Type = NodeType.LayoutSubtitution;

            if (metadata.NodeType == FragmentType.GlobalSubstitution)
                nodeToWrite.Type = NodeType.GlobalSubstitution;


            PushNode(nodeToWrite);
        }

        private void PushNode(PreviewNode nodeToWrite)
        {
            if (RootNode == null)
                RootNode = nodeToWrite;
            else
                CurrentNode.ChildNodes.Add(nodeToWrite);

            ParentNode = CurrentNode;
            CurrentNode = nodeToWrite;
        }


        public void EndWriting()
        {
            CurrentNode = ParentNode;
        }

        public void Write(string html)
        {
            CurrentNode.ChildNodes.Add(new PreviewNode {Type=NodeType.Html,Html=html });
        }
    }

}
