using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WarpCore.Cms.Routing
{
    public static class SlugGenerator
    {
        public static string Generate(string text)
        {
            Regex regex = new Regex(@"[\s,:.;\/\\&$+@# <>\[\]{}^%]+");
            return regex.Replace(text, "-").ToLower();


            //ampersand("&")
            //dollar("$")
            //plus sign("+")
            //comma(",")
            //forward slash("/")
            //colon(":")
            //semi - colon(";")
            //equals("=")
            //question mark("?")
            //'At' symbol("@")
            //pound("#").
            //    The characters generally considered unsafe are:

            //space(" ")
            //less than and greater than("<>")
            //open and close brackets("[]")
            //open and close braces("{}")
            //pipe("|")
            //backslash("\")
            //caret("^")
            //percent("%")
        }
    }
}
