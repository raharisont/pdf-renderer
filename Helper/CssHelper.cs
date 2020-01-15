using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace PdfRenderer.Helper
{
    public class CssHelper
    {
        public static List<string> GetAllCssPaths(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var cssFiles = doc.DocumentNode
                ?.SelectNodes("//*[contains(translate(@rel, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'stylesheet')]")
                ?.Select(x => x.Attributes["href"].Value)
                ?.ToList();
            return cssFiles;
        }
    }
}