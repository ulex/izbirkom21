using ExCSS;
using HtmlAgilityPack;

namespace Izbirkom21
{
  public static class Trash
  {
    public static void RemoveTrashNodes(HtmlDocument htmlDocument)
    {
      // remove trash nodes
      foreach (var span in htmlDocument.DocumentNode.SelectNodes("//span"))
      {
        var style = span.GetAttributeValue("style", "");
        if (string.IsNullOrEmpty(style)) continue;
        if (IsTrashStyle("* {" + style + "}"))
        {
          span.Remove();
        }
      }
    }

    public static bool IsTrashStyle(string stylesheetStr)
    {
      var stylesheetParser = new StylesheetParser();
      var stylesheet = stylesheetParser.Parse(stylesheetStr);
      foreach (var styleRule in stylesheet.StyleRules)
      {
        var style = styleRule.Style;
        if (IsTrashStyle(style)) return true;
      }

      return false;
    }

    public static bool IsTrashStyle(StyleDeclaration style)
    {
      return style.Position == "absolute" || style.Display == "none" || style.FontSize == "0" ||
             style.Color == "white" || style.Overflow == "hidden";
    }
  }
}