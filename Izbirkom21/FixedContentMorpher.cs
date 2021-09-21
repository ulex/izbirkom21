using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExCSS;
using HtmlAgilityPack;

namespace Izbirkom21
{
  public class FixedContentMorpher
  {
    // css class name -> replace text
    Dictionary<string, Func<string, string>> _classToMorpher = new();

    public void VisitStyleRule(IStyleRule styleRule)
    {
      var content = styleRule.Style.Content;
      var isTrashStyle = Trash.IsTrashStyle(styleRule.Style);
      if (!string.IsNullOrEmpty(content) || isTrashStyle)
      {
        var toReplace = isTrashStyle ? "" : content.Trim('"');
        foreach (Match match in Regex.Matches(styleRule.SelectorText, "\\.([\\w_]+)"))
        {
          var className = match.Groups[0].Value.Trim('.');
          if (className.Length <= 7) // 🦄
          {
            _classToMorpher.Add(className, _ => toReplace);
          }
        }
      }
    }

    public void ReplaceFixedContent(HtmlDocument htmlDocument, FontReplacer fonts)
    {
      // modify content (reverse to process nested first)
      foreach (var node in htmlDocument.DocumentNode.SelectNodes("//span|//b|//td").Reverse())
      {
        var cls = node.GetAttributeValue("class", "");
        if (_classToMorpher.TryGetValue(cls, out var replaceTo))
        {
          node.InnerHtml = replaceTo(node.InnerText);
          node.Attributes.Remove("class");
        }

        // custom fonts
        fonts.TranslateNode(cls, node);
      }
    }
  }
}