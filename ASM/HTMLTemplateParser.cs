using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ASM.Lib
{
    public sealed class HTMLTemplateParser
    {
        public static Tuple<string, Dictionary<string,string>> Parse(string htmlString)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);
            var name = doc.DocumentNode.SelectSingleNode("//meta[@name = 'arma:PresetName']").GetAttributeValue("content","MISSING");
            var modElements = doc.DocumentNode.SelectNodes("//tr[@data-type='ModContainer']").ToList();
            var modIds = modElements.ToDictionary(
                x => x.SelectSingleNode(".//a").InnerText.Replace("http://steamcommunity.com/sharedfiles/filedetails/?id=", "").Replace("https://steamcommunity.com/sharedfiles/filedetails/?id=", ""),
                x => x.SelectSingleNode(".//td").InnerText
            );

            return new(name, modIds);
        }
    }
}