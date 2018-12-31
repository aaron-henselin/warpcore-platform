using System.Linq;
using System.Text;

namespace WarpCore.Web.PageCompositor
{
    public class PageCompositor
    {
        private readonly PageRendering _pageDefinition;
        private readonly CompositableContent _contentToComposite;

        public PageCompositor(PageRendering pageDefinition, CompositableContent contentToComposite)
        {
            _pageDefinition = pageDefinition;
            _contentToComposite = contentToComposite;
        }

        public CompositedPage Composite(PageRenderMode renderMode)
        {
            var page = new CompositedPage();
            var sb = new StringBuilder();
            Render(_pageDefinition.Rendering, renderMode, sb);
            page.Html = sb.ToString();
            return page;
        }

        private void Render(PartialPageRendering pp,PageRenderMode renderMode, StringBuilder local)
        {
            var parts = _contentToComposite.WidgetContent[pp.ContentId];

            for (var index = 0; index < parts.Count; index++)
            {
                var part = parts[index];
                var renderDesignElements = !pp.IsFromLayout && renderMode == PageRenderMode.PageDesigner;

                if (part is HtmlOutput)
                {
                    if (renderDesignElements && index == 0)
                    {
                        var layoutHandle = $@"
                            <li class='StackedListItem StackedListItem--isDraggable wc-layout-handle' tabindex='1'
                                data-wc-page-content-id='{pp.ContentId}'>
                                <div class='StackedListContent'>
                                    <h4 class='Heading Heading--size4 text-no-select'>

                                        <span class='glyphicon glyphicon-cog wc-edit-command configure'
                                            data-wc-widget-type='<%# HandleName %>' 
                                            data-wc-editing-command-configure='{pp.ContentId}'>
                                        </span>
                                        <span class='glyphicon glyphicon-remove wc-edit-command delete pull-right' 
                                             data-wc-editing-command-delete='{pp.ContentId}'>
                                        </span>
                                        
                                        {pp.FriendlyName}
                                    </h4>
                                    <div class='DragHandle'></div>
                                    <div class='Pattern Pattern--typeHalftone'></div>
                                    <div class='Pattern Pattern--typePlaced'></div>
                                </div>
                            </li>
                        ";
                        local.Append(layoutHandle);
                        local.Append(
                            $"<wc-widget-render data-wc-layout='{pp.PlaceHolders.Any()}' data-wc-page-content-id='{pp.ContentId}'>");
                    }

                    local.Append(((HtmlOutput) part).sb);

                    if (renderDesignElements && index == parts.Count-1)
                    {
                        local.Append("</wc-widget-render>");
                    }
                }

                if (part is GlobalSubstitutionOutput)
                {
                    _contentToComposite.WidgetContent
                }

                if (part is LayoutSubstitutionOutput)
                {
                    var subPart = (LayoutSubstitutionOutput) part;
                    var relevantPlaceHolder = pp.PlaceHolders[subPart.Id];

                    if (renderDesignElements)
                        local.Append(
                            $"<wc-droptarget data-wc-placeholder-id='{relevantPlaceHolder.Id}' data-wc-layout-builder-id='{pp.LayoutBuilderId}'>");

                    foreach (var item in relevantPlaceHolder.Renderings)
                        Render(item, renderMode, local);

                    if (renderDesignElements)
                        local.Append("</wc-droptarget>");
                }
            }
        }
    }
}