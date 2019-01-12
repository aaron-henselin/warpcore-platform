using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.Cms.Features.Presentation.Page.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public class SwitchingHtmlWriter : StringWriter
    {
        private readonly Stack<Guid> _idStack = new Stack<Guid>();
        public readonly Dictionary<Guid, RenderingResult> output = new Dictionary<Guid, RenderingResult>();

        public void BeginWriting(PageCompositionElement pp)
        {
            if (pp.ContentId == Guid.Empty)
                throw new ArgumentException($"Invalid page content id of '{pp.ContentId}' for {pp.FriendlyName}");

            var id = pp.ContentId;

            if (_idStack.Count > 0)
            {
                var sb = this.GetStringBuilder();
                if (!output[_idStack.Peek()].InlineRenderingFragments.Any())
                    output[_idStack.Peek()].InlineRenderingFragments.Add(new HtmlOutput(sb));

                sb.Clear();
            }

            _idStack.Push(id);

            if (output.ContainsKey(id))
                throw new Exception($"Output for CmsPageContent {id} has already been recorded.");

            output.Add(id, new RenderingResult());

        }

        public void AddLayoutSubsitution(string id)
        {
            var sb = this.GetStringBuilder();
            output[_idStack.Peek()].InlineRenderingFragments.Add(new HtmlOutput(sb));
            sb.Clear();

            output[_idStack.Peek()].InlineRenderingFragments.Add(new LayoutSubstitutionOutput { Id = id });
        }

        public void AddGlobalSubsitution(string id)
        {
            var sb = this.GetStringBuilder();
            output[_idStack.Peek()].InlineRenderingFragments.Add(new HtmlOutput(sb));
            sb.Clear();

            output[_idStack.Peek()].InlineRenderingFragments.Add(new GlobalSubstitutionOutput { Id = id });
        }

        public void EndWriting()
        {
            var id = _idStack.Pop();

            var sb = this.GetStringBuilder();
            output[id].InlineRenderingFragments.Add(new HtmlOutput(sb));
            sb.Clear();

        }


    }
}