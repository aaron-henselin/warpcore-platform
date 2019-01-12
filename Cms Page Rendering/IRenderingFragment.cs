using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public static class KnownPhysicalFileExtensions
    {
        public static string MasterPage = "master";
        public static string UserControl = "ascx";
        public static string Razor = "cshtml";
    }


    public interface IRenderingFragment
    {
    }

    public class RenderingResult
    {
        public List<IRenderingFragment> InlineRenderingFragments { get; set; } = new List<IRenderingFragment>();
        public Dictionary<string, List<string>> GlobalRendering { get; set; } = new Dictionary<string, List<string>>();
    }

    public class RenderingFragmentCollection
    {
        public Dictionary<Guid, RenderingResult> RenderingResults { get; set; } = new Dictionary<Guid, RenderingResult>();
        //public Dictionary<string, List<string>> GlobalRendering { get; set; } = new Dictionary<string, List<string>>();

        public void Add(RenderingFragmentCollection collection)
        {
            foreach (var kvp in collection.RenderingResults)
                RenderingResults.Add(kvp.Key,kvp.Value);

            //foreach (var kvp in GlobalRendering)
            //{
            //    if (!GlobalRendering.ContainsKey(kvp.Key))
            //        GlobalRendering.Add(kvp.Key,new List<string>());

            //    GlobalRendering[kvp.Key].AddRange(kvp.Value);
            //}

        }
    }

    [DebuggerDisplay("GlobalPlaceHolder = {" + nameof(Id) + "}")]
    public class GlobalSubstitutionOutput : IRenderingFragment
    {
        public string Id { get; set; }
    }

    [DebuggerDisplay("LayoutPlaceHolder = {" + nameof(Id) + "}")]
    public class LayoutSubstitutionOutput : IRenderingFragment
    {
        public string Id { get; set; }
    }

    [DebuggerDisplay("Html = {" + nameof(Html) + "}")]
    public class HtmlOutput : IRenderingFragment
    {
        public string Html;

        public HtmlOutput(StringBuilder sb)
        {
            this.Html = sb.ToString();
        }
        public HtmlOutput(string sb)
        {
            this.Html = sb;
        }
    }


}
