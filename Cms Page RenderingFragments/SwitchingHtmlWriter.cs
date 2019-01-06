using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.Cms.Features.Presentation.PageComposition.Elements;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public class SwitchingHtmlWriter : StringWriter
    {
        private readonly Stack<Guid> _idStack = new Stack<Guid>();

        public readonly Dictionary<Guid, List<IRenderingFragment>> output = new Dictionary<Guid, List<IRenderingFragment>>();



        public void BeginWriting(PageCompositionElement pp)
        {
            if (pp.ContentId == Guid.Empty)
                throw new ArgumentException($"Invalid page content id of '{pp.ContentId}' for {pp.FriendlyName}");

            var id = pp.ContentId;

            if (_idStack.Count > 0)
            {
                var sb = this.GetStringBuilder();
                if (!output[_idStack.Peek()].Any())
                    output[_idStack.Peek()].Add(new HtmlOutput(sb));

                sb.Clear();
            }

            _idStack.Push(id);

            if (output.ContainsKey(id))
                throw new Exception($"Output for CmsPageContent {id} has already been recorded.");

            output.Add(id, new List<IRenderingFragment>());

        }

        public void AddLayoutSubsitution(string id)
        {
            var sb = this.GetStringBuilder();
            output[_idStack.Peek()].Add(new HtmlOutput(sb));
            sb.Clear();

            output[_idStack.Peek()].Add(new LayoutSubstitutionOutput { Id = id });
        }

        public void AddGlobalSubsitution(string id)
        {
            var sb = this.GetStringBuilder();
            output[_idStack.Peek()].Add(new HtmlOutput(sb));
            sb.Clear();

            output[_idStack.Peek()].Add(new GlobalSubstitutionOutput { Id = id });
        }

        public void EndWriting()
        {
            var id = _idStack.Pop();

            var sb = this.GetStringBuilder();
            output[id].Add(new HtmlOutput(sb));
            sb.Clear();

        }


    }
}