using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cms
{
    public static class Templating
    {
        public static string CreateToStringExpression(string variableName)
        {
            return "{" + variableName + "}";
        }

        public static string Interpolate(string template, IDictionary<string,string> dataContext)
        {
            string html = template;
            var findVariablesRegEx = new Regex(@"\{\S*?\}");
            var matches = findVariablesRegEx.Matches(template);
            foreach (Match match in matches)
            {
                foreach (Capture capture in match.Captures)
                {
                    var expression = capture.Value.Substring(1, capture.Value.Length - 2);
                    html = html.Replace(capture.Value, dataContext[expression]);
                }
            }
            return html;
        }
    }
}
