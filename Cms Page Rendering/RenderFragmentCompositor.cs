using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public class RenderFragmentCompositor
    {
        private readonly PageComposition _pageDefinition;
        private readonly RenderingFragmentCollection _page;

        private readonly PageDesignerHtmlFactory _pageDesignerHtmlFactory =
            Dependency.Resolve<PageDesignerHtmlFactory>();

        private ILookup<string, List<string>> _globals;

        public RenderFragmentCompositor(PageComposition pageDefinition, RenderingFragmentCollection page)
        {
            _pageDefinition = pageDefinition;
            _page = page;

            _globals = _page.RenderingResults
                .SelectMany(x => x.Value.GlobalRendering)
                .ToLookup(x => x.Key, x=> x.Value);
        }

        public CompositedResponse Compose(FragmentRenderMode renderMode)
        {
            var page = new CompositedResponse();
            var sb = new StringBuilder();
            
            Render(_pageDefinition.RootElement, new RenderAttributes {Mode=renderMode}, sb);
            page.Html = sb.ToString();
            return page;
        }

        private class RenderAttributes
        {
            public FragmentRenderMode Mode { get; set; }
            public bool IsFirstContentPart { get; set; }
            public bool IsLastContentPart { get; set; }
        }
        

        private void Render(PageCompositionElement pp,RenderAttributes attributes, StringBuilder local)
        {
            if (!_page.RenderingResults.ContainsKey(pp.ContentId))
                throw new Exception("There is no rendering fragment available for " + pp.FriendlyName + " with content id " +pp.ContentId);

            

            var renderingResultForElement = _page.RenderingResults[pp.ContentId];

            for (var index = 0; index < renderingResultForElement.InlineRenderingFragments.Count; index++)
            {
                var part = renderingResultForElement.InlineRenderingFragments[index];
                var isFirst = index == 0;
                var isLast = index == renderingResultForElement.InlineRenderingFragments.Count - 1;
                

                if (part is HtmlOutput)
                {

                    var htmlOutput = (HtmlOutput) part;
                    var renderAttributes = new RenderAttributes
                    {
                        Mode = attributes.Mode,
                        IsFirstContentPart = isFirst,
                        IsLastContentPart = isLast
                    };

                    WriteHtmlOutput(pp, htmlOutput, renderAttributes, local);
                    continue;
                }

                if (part is GlobalSubstitutionOutput)
                {
                    var global = (GlobalSubstitutionOutput) part;
                    foreach (var s in _globals[global.Id])
                        local.Append(s);

                    if (attributes.Mode == FragmentRenderMode.PageDesigner)
                    {
                        if (global.Id == GlobalLayoutPlaceHolderIds.Head)
                        {
                            //todo: add back in the head where possible.
                            //var head = _pageDesignerHtmlFactory.LoadPageDesignerHead();
                            //var style = _pageDesignerHtmlFactory.LoadPageDesignerStyle();
                            //local.Append(head);
                            //local.Append(style);

                        }

                        if (global.Id == GlobalLayoutPlaceHolderIds.Scripts)
                        {
                            var head = _pageDesignerHtmlFactory.LoadPageDesignerHead();
                            var style = _pageDesignerHtmlFactory.LoadPageDesignerStyle();
                            local.Append(head);
                            local.Append(style);

                            var scripts = _pageDesignerHtmlFactory.LoadPageDesignerScripts();
                            local.Append(scripts);
                        }

                        if (global.Id == GlobalLayoutPlaceHolderIds.InternalStateTracking)
                        {


                            var tags = _pageDesignerHtmlFactory.CreatePageDesignerStateTags();
                            local.Append(string.Join(string.Empty,tags));
                        }


                    }

                    continue;
                }

                if (part is LayoutSubstitutionOutput)
                {
                    var subPart = (LayoutSubstitutionOutput) part;
                    var relevantPlaceHolder = pp.PlaceHolders[subPart.Id];
                    var renderAttributes = new RenderAttributes
                    {
                        Mode = attributes.Mode,
                        IsFirstContentPart = isFirst,
                        IsLastContentPart = isLast
                    };

                    WriteSubstitution(pp, renderAttributes, relevantPlaceHolder, local);
                    continue;
                }

                throw new ArgumentException();
            }
        }

        private void WriteSubstitution(PageCompositionElement pp, RenderAttributes renderAttributes, RenderingsPlaceHolder relevantPlaceHolder, StringBuilder local)
        {
            var renderDesignElements = !pp.IsFromLayout && renderAttributes.Mode == FragmentRenderMode.PageDesigner;

            if (renderDesignElements)
            {
                var dropTargetBegin = _pageDesignerHtmlFactory.CreateDropTargetBeginMarker(pp, relevantPlaceHolder);
                local.Append(dropTargetBegin);
            }

            foreach (var item in relevantPlaceHolder.Renderings)
                Render(item, renderAttributes, local);

            if (renderDesignElements)
            {
                var dropTargetEnd = _pageDesignerHtmlFactory.CreateDropTargetEndMarker();
                local.Append(dropTargetEnd);
            }
        }

        private void WriteHtmlOutput(PageCompositionElement pp, HtmlOutput htmlOutput, RenderAttributes renderAttributes,
            StringBuilder local)
        {
            var renderDesignElements = !pp.IsFromLayout && renderAttributes.Mode == FragmentRenderMode.PageDesigner;
            if (renderDesignElements && renderAttributes.IsFirstContentPart)
            {
                var layoutHandle = _pageDesignerHtmlFactory.CreateLayoutHandle(pp);
                local.Append(layoutHandle);

                var widgetBeginMarker = _pageDesignerHtmlFactory.CreateWidgetBeginMarker(pp);
                local.Append(widgetBeginMarker);
            }

            local.Append(htmlOutput.Html);

            if (renderDesignElements && renderAttributes.IsLastContentPart)
            {
                var widgetEndMarker = _pageDesignerHtmlFactory.CreateWidgetEndMarker(pp);
                local.Append(widgetEndMarker);
            }
        }
    }
}