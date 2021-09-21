using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExCSS;
using HtmlAgilityPack;

namespace Izbirkom21
{
  public static class Program
  {
    static int Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("use:");
        Console.WriteLine("    dotnet run <saved html page> [output]");
        return -1;
      }

      var htmlDocument = new HtmlDocument();
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      htmlDocument.Load(File.OpenRead(args[0]), Encoding.GetEncoding("windows-1251"));

      var cssParser = new StylesheetParser();
      var fonts = new FontReplacer();
      var content = new FixedContentMorpher();

      var styles = htmlDocument.DocumentNode.SelectNodes("//style");
      foreach (var style in styles)
      {
        var sheet = cssParser.Parse(style.InnerHtml);
        fonts.VisitFontFaces(sheet);

        foreach (var styleRule in sheet.StyleRules)
        {
          fonts.VisitStyleRule(styleRule);

          content.VisitStyleRule(styleRule);
        }
      }

      RemoveAndExecuteScripts(htmlDocument);

      // temporary file for debugging purposes
      //htmlDocument.Save("out_ns.html", Encoding.UTF8);

      Trash.RemoveTrashNodes(htmlDocument);

      content.ReplaceFixedContent(htmlDocument, fonts);

      FinalCleanup(htmlDocument);

      if (args.Length >= 2)
        htmlDocument.Save(args[1], Encoding.UTF8);
      
      return 0;
    }

    private static void FinalCleanup(HtmlDocument htmlDocument)
    {
      // unwrapp all remaining trash-like nodes to simplify parsing
      foreach (var b in htmlDocument.DocumentNode.SelectNodes("//td"))
      {
        var text = b.InnerText;
        b.RemoveAllChildren();
        if (!string.IsNullOrEmpty(text))
          b.AppendChild(HtmlNode.CreateNode(text));
      }

      foreach (var b in htmlDocument.DocumentNode.SelectNodes("//b"))
      {
        b.ParentNode.ReplaceChild(HtmlNode.CreateNode(b.InnerText), b);
      }
    }

    private static void RemoveAndExecuteScripts(HtmlDocument htmlDocument)
    {
      var classToNodesLookup = new Dictionary<string, List<HtmlNode>>();
      foreach (var node in htmlDocument.DocumentNode.Descendants())
      {
        if (node.Name == "span" || node.Name == "b" || node.Name == "td" || node.Name == "nobr")
        {
          foreach (var cls in node.GetAttributeValue("class", "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
          {
            var list = classToNodesLookup.TryGetValue(cls, out var l) ? l : classToNodesLookup[cls] = new List<HtmlNode>();
            list.Add(node);
          }
        }
      }

      var mainTable = htmlDocument.DocumentNode.SelectSingleNode("//table[contains(@class, 'table-sm')]");
      // remove and execute scripts
      foreach (var script in htmlDocument.DocumentNode.SelectNodes("//script"))
      {
        var scriptInnerHtml = script.InnerHtml.Split(';');
        foreach (var str in scriptInnerHtml)
        {
          var match = Regex.Match(str, "^ ?\\w{3}_\\w{3}\\(([^,]*),([^,]*),([^,]*)\\)");
          if (match.Success)
          {
            var g1 = match.Groups[1].Value;
            var g2 = match.Groups[2].Value;
            var className = g1.Trim('\'', ' ');

            if (int.TryParse(g1.Trim('\'', ' '), out var td1) && int.TryParse(g2.Trim('\'', ' '), out var td2))
            {
              var node1 = mainTable.Descendants("td").Skip(td1).First();
              var node2 = mainTable.Descendants("td").Skip(td2).First();
              (node1.InnerHtml, node2.InnerHtml) = (node2.InnerHtml, node1.InnerHtml);
            }
            else if (int.TryParse(g2, out var result))
            {
              MorphElementsByStyleName(classToNodesLookup, className, RemoveChar(result));
            }
            else
            {
              MorphElementsByStyleName(classToNodesLookup, className, _ => g2.Trim('\'', ' '));
            }
          }
        }

        script.Remove();
      }

      // this loader obscure resulting table
      foreach (var loader in htmlDocument.DocumentNode.SelectNodes("//div")
        .Where(d => d.GetAttributeValue("class", "") == "loader-template"))
      {
        loader.Remove();
      }

      // cleanup
      foreach (var loader in htmlDocument.DocumentNode.SelectNodes("//link|//style|//img"))
      {
        loader.Remove();
      }
      foreach (var table in htmlDocument.DocumentNode.SelectNodes("//table"))
      {
        table.SetAttributeValue("style", table.GetAttributeValue("style", "").Replace("opacity: 0;", "").Replace("visibility: hidden;", ""));
      }
    }

    private static void MorphElementsByStyleName(IReadOnlyDictionary<string, List<HtmlNode>> classToNodesLookup, string className, Func<string, string> morpher)
    {
      if (classToNodesLookup.TryGetValue(className, out var list))
      {
        foreach (var node in list)
        {
          node.InnerHtml = morpher(node.InnerHtml);
        }
      }
      else
        Console.Error.WriteLine($"Node by class name `{className}` not found. It is likely an error ");
    }

    private static Func<string, string> RemoveChar(int index)
    {
      if (index < 0) return s => s.Remove(s.Length - 1, 1);
      return s => s.Remove(index, 1);
    }
  }
}
