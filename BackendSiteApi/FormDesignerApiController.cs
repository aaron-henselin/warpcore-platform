using System;
using System.Linq;
using System.Web.Http;
using BlazorComponents.Shared;
using Cms.Forms;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Context;
using Modules.Cms.Features.Presentation.PageComposition;
using WarpCore.Cms;
using WarpCore.Cms.Toolbox;
using WarpCore.Platform.Orm;

namespace BackendSiteApi
{
    public class FormDesignerPreviewWriter
    {
        private int _seq = 0;
        public PreviewNode Write(PageStructure pageStructure)
        {
            var rootNode = new PreviewNode
            {
                Type = NodeType.Element,
                IsFromLayout = true,
                PreviewNodeId = ToGuid(_seq++),
            };

            var bodyNode = new PreviewNode
            {
                Type = NodeType.LayoutSubtitution,
                IsFromLayout = true,
                PreviewNodeId = ToGuid(_seq++),
            };

            rootNode.ChildNodes.Add(bodyNode);

            foreach (var node in pageStructure.ChildNodes)
                bodyNode.ChildNodes.Add(Write(node));

            return rootNode;
        }

        private PreviewNode Write(StructureNode pageStructure)
        {
            
            var friendlyName = string.Empty;
            if (pageStructure.WidgetTypeCode != null)
            {
                friendlyName = pageStructure.WidgetTypeCode;
                //friendlyName = new ToolboxManager().GetToolboxItemByCode(pageStructure.WidgetTypeCode).FriendlyName;
            }

            var previewNode = new PreviewNode
            {
                ContentId = pageStructure.Id,
                Type = NodeType.Element,
                Parameters = pageStructure.Parameters,
                PreviewNodeId = ToGuid(_seq++),
                FriendlyName = friendlyName
            };

            var isRow = "ConfiguratorRow" == pageStructure.WidgetTypeCode;
            if (isRow)
            {
                previewNode.ChildNodes.Add(new PreviewNode {Type = NodeType.Html, Html = "<div class='row'>"});

                var numColumnsRaw = pageStructure.Parameters["NumColumns"];
                int columnCount;
                var isNum = Int32.TryParse(numColumnsRaw, out columnCount);
                if (!isNum)
                    return previewNode;

                for (int i = 0; i < columnCount; i++)
                {
                    var i1 = i;

                    previewNode.ChildNodes.Add(new PreviewNode
                    {
                        Type = NodeType.Html,
                        Html = "<div class='col'>",
                        PreviewNodeId = ToGuid(_seq++)
                    });
                    var newSubstitution = new PreviewNode
                    {
                        Type = NodeType.LayoutSubtitution,
                        PlaceHolderId = i1.ToString(),
                        PreviewNodeId = ToGuid(_seq++)
                    };
                    previewNode.ChildNodes.Add(newSubstitution);

                    var nodesToPreview =
                        pageStructure.ChildNodes.Where(x => x.PlacementContentPlaceHolderId == i1.ToString());
                    newSubstitution.ChildNodes = nodesToPreview.Select(Write).ToList();
                    previewNode.ChildNodes.Add(new PreviewNode
                    {
                        Type = NodeType.Html,
                        Html = "</div>",
                        PreviewNodeId = ToGuid(_seq++)
                    });
                }

                previewNode.ChildNodes.Add(new PreviewNode
                {
                    Type = NodeType.Html,
                    Html = "</div>",
                    PreviewNodeId = ToGuid(_seq++)
                });
            }
            else
            {
                previewNode.UseClientRenderer = true;
            }

            return previewNode;

            //PlaceHolderId = pageStructure.
            //    FriendlyName = pageStructure.WidgetTypeCode

            //ChildNodes = pageStructure.ChildNodes.Select(Write).ToList()
        }

        private static Guid ToGuid(int value)
        {
            byte[] bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }
    }

    public class FormDesignerApiController : ApiController
    {
        [HttpGet]
        [Route("api/design/form/{pageId}/preview")]
        public PreviewNode Page(Guid formId)
        {
            var draft = new FormRepository().FindContentVersions(By.ContentId(formId), ContentEnvironment.Draft).Result.Single();
            var structureNodeConverter = new StructureNodeConverter();
            var structure = structureNodeConverter.GetPageStructure(draft);
            return new FormDesignerPreviewWriter().Write(structure);
        }

        [HttpPost]
        [Route("api/design/form/{formId}/preview")]
        public PreviewNode Page(Guid formId, PageStructure pageStructure)
        {
            return new FormDesignerPreviewWriter().Write(pageStructure);
        }

        [HttpPost]
        [Route("api/design/form/{formId}/draft")]
        public void SaveDraft(Guid formId, PageStructure pageStructure)
        {
            var pageRepository = new FormRepository();
            var draft = pageRepository.FindContentVersions(By.ContentId(formId), ContentEnvironment.Draft).Result.Single();
            new StructureNodeConverter().ApplyNewStructureToCmsPage(draft, pageStructure);
            pageRepository.Save(draft);
        }

        [HttpPost]
        [Route("api/design/form/{formId}/live")]
        public void SaveAndPublish(Guid formId, PageStructure pageStructure)
        {
            var pageRepository = new FormRepository();
            var draft = pageRepository.FindContentVersions(By.ContentId(formId), ContentEnvironment.Draft).Result.Single();
            new StructureNodeConverter().ApplyNewStructureToCmsPage(draft, pageStructure);
            pageRepository.Save(draft);
            pageRepository.Publish(By.ContentId(draft.ContentId));
        }

        [HttpGet]
        [Route("api/design/form/{formId}/structure")]
        public PageStructure PageStructure(Guid formId)
        {
            var draft = new FormRepository().FindContentVersions(By.ContentId(formId), ContentEnvironment.Draft).Result.Single();
            return new StructureNodeConverter().GetPageStructure(draft);
        }
    }
}