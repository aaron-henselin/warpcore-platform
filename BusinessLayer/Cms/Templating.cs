using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cms
{
    public class TemplatingException : Exception
    {
        public TemplatingException(string message):base(message)
        {
        }
    }

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

                    if (!dataContext.ContainsKey(expression))
                        throw new TemplatingException("Data context does not contain a value for '"+expression+"'");

                    html = html.Replace(capture.Value, dataContext[expression]);
                }
            }
            return html;
        }
    }
}
