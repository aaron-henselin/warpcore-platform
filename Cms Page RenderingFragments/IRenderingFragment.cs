using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Modules.Cms.Featues.Presentation.PageFragmentRendering
{
    public static class KnownPhysicalFileExtensions
    {
        public static string MasterPage = "master";
        public static string Razor = "cshtml";
    }


    public interface IRenderingFragment
    {
    }


    public class RenderingFragmentCollection
    {
        public Dictionary<Guid, List<IRenderingFragment>> WidgetContent { get; set; } = new Dictionary<Guid, List<IRenderingFragment>>();
        public Dictionary<string, List<string>> GlobalContent { get; set; } = new Dictionary<string, List<string>>();

        public void Add(RenderingFragmentCollection collection)
        {
            foreach (var kvp in collection.WidgetContent)
                WidgetContent.Add(kvp.Key,kvp.Value);

            foreach (var kvp in GlobalContent)
            {
                if (!GlobalContent.ContainsKey(kvp.Key))
                    GlobalContent.Add(kvp.Key,new List<string>());

                GlobalContent[kvp.Key].AddRange(kvp.Value);
            }

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
    }


}
