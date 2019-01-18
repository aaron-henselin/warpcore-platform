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
        [Route("api/design")]
        public Node Page(Guid pageId)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();
            var page = new PageCompositionBuilder().CreatePageComposition(draft,PageRenderMode.PageDesigner);


            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page, FragmentRenderMode.PageDesigner);
            var compositor = new RenderFragmentCompositor(page, batch);

            var treeWriter= new TreeHtmlWriter();

            compositor.WriteComposedFragments(FragmentRenderMode.PageDesigner,treeWriter);

            return treeWriter.RootNode;
        }
    }

    public class TreeHtmlWriter : ComposedHtmlWriter
    {
        public Node RootNode { get; set; }

        public Node ParentNode { get; set; }
        public Node CurrentNode { get; set; }

        public void BeginWriting(CompostedContentMetdata metadata)
        {
            var nodeToWrite = new Node();
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

        private void PushNode(Node nodeToWrite)
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
            CurrentNode.ChildNodes.Add(new Node {Type=NodeType.Html,Html=html });
        }
    }

}
