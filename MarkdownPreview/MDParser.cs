using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using CommonMark;

namespace MarkdownPreview
{
    class MDParser
    {
        public static string Parse(string markdown)
        {
            return CommonMarkConverter.Convert(markdown);
        }

        public static string GetHeader(bool dark)
        {
            string header = "<!DOCTYPE html><html><head><title>Page</title><style>body { ";

            header += dark ? "background-color:black;color:white" : "background-color:white;color:black;";

            header += " }";
            header += GetStyle();
            header += "</style></head><body>";

            return header;
        }

        public static string GetFooter()
        {
            return "</body></html>";
        }

        private static string GetStyle()
        {
            return "" +
                "body {" +
                "font-family: sans-serif;" +
                "}";
        }
    }
}
