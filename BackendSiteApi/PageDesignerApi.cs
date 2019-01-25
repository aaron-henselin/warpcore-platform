using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using BlazorComponents.Shared;
using Cms.Forms;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.Page.Elements;
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
        [Route("api/design/page/{pageId}/preview")]
        public PreviewNode Page(Guid pageId)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();
            var page = new PageCompositionBuilder().CreatePageComposition(draft, PageRenderMode.PageDesigner);
            var cre = new BatchingFragmentRenderer();
            var batch = cre.Execute(page, FragmentRenderMode.PageDesigner);
            var compositor = new RenderFragmentCompositor(page, batch);
            var treeWriter = new PagePreviewWriter(draft);

            compositor.WriteComposedFragments(FragmentRenderMode.PageDesigner, treeWriter);
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
            var treeWriter = new PagePreviewWriter(draft);

            compositor.WriteComposedFragments(FragmentRenderMode.PageDesigner, treeWriter);
            return treeWriter.RootNode;
        }

        [HttpPost]
        [Route("api/design/page/{pageId}/draft")]
        public void SaveDraft(Guid pageId, PageStructure pageStructure)
        {
            var pageRepository = new CmsPageRepository();
            var draft = pageRepository.FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();

            new StructureNodeConverter().ApplyNewStructureToCmsPage(draft, pageStructure);

            pageRepository.Save(draft);
        }

        [HttpPost]
        [Route("api/design/page/{pageId}/live")]
        public void SaveAndPublish(Guid pageId, PageStructure pageStructure)
        {
            var pageRepository = new CmsPageRepository();
            var draft = pageRepository.FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();

            new StructureNodeConverter().ApplyNewStructureToCmsPage(draft, pageStructure);

            pageRepository.Save(draft);
            pageRepository.Publish(By.ContentId(draft.ContentId));
        }

        [HttpGet]
        [Route("api/design/page/{pageId}/structure")]
        public PageStructure PageStructure(Guid pageId)
        {
            var draft = new CmsPageRepository().FindContentVersions(By.ContentId(pageId), ContentEnvironment.Draft).Result.Single();
            return new StructureNodeConverter().GetPageStructure(draft);
        }

        [HttpGet]
        [Route("api/design/configurator-form/{widgetTypeCode}")]
        public ConfiguratorFormDescription GetConfiguratorForm(string widgetTypeCode)
        {
            var toolboxItem = new ToolboxManager().GetToolboxItemByCode(widgetTypeCode);
            var toolboxItemNativeType = new CmsPageContentActivator().GetToolboxItemNativeType(toolboxItem);
            var defaultForm = new ConfiguratorCmsPageContentBuilder().GenerateDefaultForm(toolboxItemNativeType);

            // new CmsPageContentActivator().GetDefaultContentParameterValues(toolboxItem)
            //.ToDictionary(x => x.Key, x => x.Value)

            var description =
                new ConfiguratorFormDescription
                {
                    Layout = new StructureNodeConverter().GetPageStructure(defaultForm),
                    DefaultValues = new Dictionary<string, string>()
                };
            return description;
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


    public class StructureNodeConverter
    {
        public void ApplyNewStructureToCmsPage(CmsPage draft, PageStructure pageStructure)
        {
            draft.PageContent = pageStructure.ChildNodes.Select(ApplyNewStructure).ToList();
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
                AllContent = node.ChildNodes.Select(ApplyNewStructure).ToList()
            };
        }


        public PageStructure GetPageStructure(IHasDesignedContent draft)
        {
            return new PageStructure
            {
                ChildNodes = draft.DesignedContent.Select(GetPageStructure).ToList()
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
                ChildNodes = content.AllContent.Select(GetPageStructure).ToList()
            };
            return sn;
        }
    }



    public class PagePreviewWriter : ComposedHtmlWriter
    {
        private readonly CmsPage _draft;
        private int _seq = 1;
        public PagePreviewWriter(CmsPage draft)
        {
            _draft = draft;
        }

        private Stack<PreviewNode> Node = new Stack<PreviewNode>();

        public PreviewNode RootNode { get; set; }

        public void BeginWriting(CompostedContentMetdata metadata)
        {
            var nodeToWrite = new PreviewNode();
            nodeToWrite.PreviewNodeId = ToGuid(_seq++);

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


        private PreviewNode CurrentNode => Node.Peek();

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

            var previewNode = new PreviewNode
            {
                PreviewNodeId = ToGuid(_seq++),
                Type = NodeType.Html,
                Html = html
            };
            CurrentNode.ChildNodes.Add(previewNode);
        }
        private static Guid ToGuid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }

}
