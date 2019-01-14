using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modules.Cms.Featues.Presentation.PageFragmentRendering;
using Modules.Cms.Features.Presentation.Page.Elements;
using WarpCore.Platform.Kernel;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public class MetadataOutputWriter
    {
        public void Write()
        {
        }
    }

    public class HtmlOutputWriter
    { 

        //private void WriteHtmlOutput(PageCompositionElement pp, HtmlOutput htmlOutput, RenderAttributes renderAttributes, StringBuilder local)
        //{
        //    var renderDesignElements = !pp.IsFromLayout && renderAttributes.Mode == FragmentRenderMode.PageDesigner;
        //    if (renderDesignElements && renderAttributes.IsFirstContentPart)
        //    {
        //        var layoutHandle = _pageDesignerHtmlFactory.CreateLayoutHandle(pp);
        //        local.Append(layoutHandle);

        //        var widgetBeginMarker = _pageDesignerHtmlFactory.CreateWidgetBeginMarker(pp);
        //        local.Append(widgetBeginMarker);
        //    }

        //    local.Append(htmlOutput.Html);

        //    if (renderDesignElements && renderAttributes.IsLastContentPart)
        //    {
        //        var widgetEndMarker = _pageDesignerHtmlFactory.CreateWidgetEndMarker(pp);
        //        local.Append(widgetEndMarker);
        //    }
        //}
    }

    

    public class RenderAttributes
    {
        public FragmentRenderMode Mode { get; set; }
        public bool IsFirstContentPart { get; set; }
        public bool IsLastContentPart { get; set; }
    }




    public class HtmlOnlyComposedHtmlWriter : ComposedHtmlWriter
    {
        private CompositedHtml _current;
        List<CompositedHtml> CompositedContent { get; set; } = new List<CompositedHtml>();

        public override string ToString()
        {
            return string.Join(string.Empty, CompositedContent.Where(x => x.Html != null).Select(x => x.Html));
        }


        public void BeginWriting(CompostedContentMetdata metadata)
        {
            _current = new CompositedHtml
            {
                Metadata = metadata,
            };
            CompositedContent.Add(_current);
        }

        public void Write(string html)
        {
            if (_current.Html == null)
                _current.Html = html;
            else
                _current.Html += html;
        }


        public void EndWriting()
        {

        }
    }

    public interface ComposedHtmlWriter
    {
        void BeginWriting(CompostedContentMetdata metadata);

        void Write(string html);

        void EndWriting();
    }

    public class CompositedHtml
    {
        public CompostedContentMetdata Metadata { get; set; }
        public string Html { get; set; }
    }

    public class CompostedContentMetdata {
        public string FriendlyName { get; set; }
        public Guid ContentId { get; set; }
        public RenderAttributes RenderAttributes { get; set; }
        public FragmentType NodeType { get; set; }
    }

    public enum FragmentType
    {
        Element,
        Html,
        LayoutSubtitution,
        GlobalSubstitution
    }

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

        public void WriteComposedFragments(FragmentRenderMode renderMode,ComposedHtmlWriter htmlWriter)
        {
            var page = new CompositedResponse();
            Render(_pageDefinition.RootElement, new RenderAttributes {Mode=renderMode}, htmlWriter);
           
        }


        private void Render(PageCompositionElement pp,RenderAttributes attributes, ComposedHtmlWriter local)
        {
            if (!_page.RenderingResults.ContainsKey(pp.ContentId))
                throw new Exception("There is no rendering fragment available for " + pp.FriendlyName + " with content id " +pp.ContentId);

            var metadata = new CompostedContentMetdata
            {
                FriendlyName = pp.FriendlyName,
                ContentId = pp.ContentId,
                RenderAttributes = attributes,
                NodeType = FragmentType.Html,
            };
            local.BeginWriting(metadata);

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
                    
                    local.Write(htmlOutput.Html);
                    

                    continue;
                }

                if (part is GlobalSubstitutionOutput)
                {
                    var global = (GlobalSubstitutionOutput)part;
                    local.BeginWriting(new CompostedContentMetdata
                    {
                        FriendlyName = global.Id,
                        NodeType = FragmentType.GlobalSubstitution
                        
                    });

                    foreach (var s in _globals[global.Id])
                        local.Write(string.Join("",s));

                    local.EndWriting();
                   


                    //if (attributes.Mode == FragmentRenderMode.PageDesigner)
                    //{
                    //    if (global.Id == GlobalLayoutPlaceHolderIds.Head)
                    //    {

                    //    }

                    //    if (global.Id == GlobalLayoutPlaceHolderIds.Scripts)
                    //    {
                    //        var head = _pageDesignerHtmlFactory.LoadPageDesignerHead();
                    //        var style = _pageDesignerHtmlFactory.LoadPageDesignerStyle();
                            
                    //        local.Append(head);
                    //        local.Append(style);

                    //        var scripts = _pageDesignerHtmlFactory.LoadPageDesignerScripts();
                    //        local.Append(scripts);
                    //    }

                    //    if (global.Id == GlobalLayoutPlaceHolderIds.InternalStateTracking)
                    //    {


                    //        var tags = _pageDesignerHtmlFactory.CreatePageDesignerStateTags();
                    //        local.Append(string.Join(string.Empty,tags));
                    //    }


                    //}

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

                 
                    local.BeginWriting(new CompostedContentMetdata
                    {
                        FriendlyName = subPart.Id,
                        RenderAttributes = renderAttributes,
                        NodeType = FragmentType.LayoutSubtitution
                    });

                    foreach (var item in relevantPlaceHolder.Renderings)
                        Render(item, renderAttributes, local);

                    local.EndWriting();

                    continue;
                }

                throw new ArgumentException();
            }

            local.EndWriting();
        }



        private void WriteSubstitution(PageCompositionElement pp, RenderAttributes renderAttributes, RenderingsPlaceHolder relevantPlaceHolder, ComposedHtmlWriter local)
        {
            //var renderDesignElements = !pp.IsFromLayout && renderAttributes.Mode == FragmentRenderMode.PageDesigner;

            //if (renderDesignElements)
            //{
            //    var dropTargetBegin = _pageDesignerHtmlFactory.CreateDropTargetBeginMarker(pp, relevantPlaceHolder);
            //    local.Append(dropTargetBegin);
            //}

            


            //foreach (var item in relevantPlaceHolder.Renderings)
            //    Render(item, renderAttributes, local);

            //if (renderDesignElements)
            //{
            //    var dropTargetEnd = _pageDesignerHtmlFactory.CreateDropTargetEndMarker();
            //    local.Append(dropTargetEnd);
            //}
        }


    }
}