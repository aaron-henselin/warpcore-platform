using System;
using System.Text;
using Modules.Cms.Features.Presentation.PageComposition;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public class RenderFragmentCompositor
    {
        private readonly Features.Presentation.PageComposition.Elements.PageComposition _pageDefinition;
        private readonly RenderingFragmentCollection _contentToComposite;
        private readonly PageDesignerHtmlFactory _pageDesignerHtmlFactory = new PageDesignerHtmlFactory();
        public RenderFragmentCompositor(Features.Presentation.PageComposition.Elements.PageComposition pageDefinition, RenderingFragmentCollection contentToComposite)
        {
            _pageDefinition = pageDefinition;
            _contentToComposite = contentToComposite;
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
            if (!_contentToComposite.WidgetContent.ContainsKey(pp.ContentId))
                throw new Exception("There is no rendering fragment available for " + pp.FriendlyName + " with content id " +pp.ContentId);
            var parts = _contentToComposite.WidgetContent[pp.ContentId];

            for (var index = 0; index < parts.Count; index++)
            {
                var part = parts[index];
                var isFirst = index == 0;
                var isLast = index == parts.Count - 1;
                

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

                    if (_contentToComposite.GlobalContent.ContainsKey(global.Id))
                    {
                        var strings = _contentToComposite.GlobalContent[global.Id];
                        foreach (var s in strings)
                            local.Append(s);
                    }


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