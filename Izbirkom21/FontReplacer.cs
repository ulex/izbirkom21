using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using ExCSS;
using HtmlAgilityPack;
using Typography.OpenFont;

namespace Izbirkom21
{
  public class FontReplacer
  {
    public string FontsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "fonts_cache");

    /// css class name -> font name (guid.ttf)
    private static Dictionary<string, string> _classToFont = new();

    /// font-family name -> font name (guid.ttf)
    private Dictionary<string, string> _fonts = new();

    /// font name (guid.ttf) -> Converter
    private Dictionary<string, Func<char, char>> _fontToConverter = new();

    public void VisitStyleRule(IStyleRule styleRule)
    {
      var ff = styleRule.Style.FontFamily;
      if (!string.IsNullOrEmpty(ff) && _fonts.TryGetValue(ff, out var fontName))
      {
        foreach (Match match in Regex.Matches(styleRule.SelectorText, "\\.([\\w_]+)"))
          _classToFont[match.Groups[0].Value.Trim('.')] = fontName;
      }
    }

    public void VisitFontFaces(Stylesheet sheet)
    {
      foreach (var fontFace in sheet.FontfaceSetRules)
      {
        var fontName = Regex.Match(fontFace.Source, "[^/]*.ttf");
        _fonts[fontFace.Family] = fontName.Value;
        _fontToConverter[fontName.Value] = ReadFont(fontName.Value);
      }
    }

    private string GetOrDownloadFont(string name, string directory)
    {
      if (!Directory.Exists(FontsDirectory))
        Directory.CreateDirectory(FontsDirectory);

      var path = Path.Combine(FontsDirectory, name);
      if (File.Exists(path))
        return path;

      HttpClient client = new HttpClient();
      client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:92.0) Gecko/20100101 Firefox/92.0");
      var fontContent = client.GetByteArrayAsync($"http://izbirkom.ru/{directory}/{name}").Result;
      File.WriteAllBytes(path, fontContent);
      return path;
    }

    private Func<char, char> ReadFont(string fontValue)
    {
      using var izbFontStream = File.OpenRead(GetOrDownloadFont(fontValue, "fonts1"));
      var izb = new OpenFontReader().Read(izbFontStream);
      using var originalFontStream = File.OpenRead(GetOrDownloadFont("pt-sans-v12-latin_cyrillic-regular.ttf", "fonts"));
      var ptsans = new OpenFontReader().Read(originalFontStream);

      List<char> alphabet = new();
      for (char l = 'A'; l <= 'Z'; l++) alphabet.Add(l);
      for (char l = 'z'; l <= 'z'; l++) alphabet.Add(l);
      for (char l = '0'; l <= '9'; l++) alphabet.Add(l);
      for (char l = 'а'; l <= 'я'; l++) alphabet.Add(l);
      for (char l = 'А'; l <= 'Я'; l++) alphabet.Add(l);

      var glyphIndexToLetter = new char[ptsans.GlyphCount];
      foreach (var c in alphabet)
        glyphIndexToLetter[ptsans.GetGlyphIndex(c)] = c;

      return c => glyphIndexToLetter[izb.GetGlyphIndex(c)];
    }

    public void TranslateNode(string cls, HtmlNode node)
    {
      if (_classToFont.TryGetValue(cls, out var font1))
      {
        var converter = _fontToConverter[font1];
        var convertedText = new string(node.InnerText.Select(converter).ToArray());
        node.InnerHtml = convertedText;
        node.Attributes.Remove("class");
      }
    }
  }
}