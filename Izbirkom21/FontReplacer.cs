using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExCSS;
using HtmlAgilityPack;
using Typography.OpenFont;

namespace Izbirkom21
{
  public interface IFontProvider
  {
    ConcurrentDictionary<object, object> Cache { get; }

    Task<byte[]> GetFont(string name, string directory);
  }

  public class CacheFontProvider : IFontProvider
  {
    public string FontsDirectory = Environment.GetEnvironmentVariable("IZ_CACHE") ?? Path.Combine(Directory.GetCurrentDirectory(), "fonts_cache");
    private readonly IFontProvider _delegateTo;

    public CacheFontProvider(IFontProvider delegateTo)
    {
      _delegateTo = delegateTo;
    }

    public ConcurrentDictionary<object, object> Cache { get; } = new ConcurrentDictionary<object, object>();

    public async Task<byte[]> GetFont(string name, string directory)
    {
      if (!Directory.Exists(FontsDirectory))
        Directory.CreateDirectory(FontsDirectory);

      var path = Path.Combine(FontsDirectory, name);
      if (File.Exists(path))
        return await File.ReadAllBytesAsync(path);

      var bytes = await _delegateTo.GetFont(name, directory);
      await File.WriteAllBytesAsync(path, bytes);

      return bytes;
    }
  }

  public class DownloadFontProvider : IFontProvider
  {
    public ConcurrentDictionary<object, object> Cache { get; } = new ConcurrentDictionary<object, object>();

    public async Task<byte[]> GetFont(string name, string directory)
    {
      var requestUri = $"http://izbirkom.ru/{directory}/{name}";
      Console.Write($"Downloading font from {requestUri}");
      var client = new HttpClient();
      client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:92.0) Gecko/20100101 Firefox/92.0");
      var fontContent = await client.GetByteArrayAsync(requestUri);
      Console.WriteLine("done");
      return fontContent;
    }
  }

  public class FontReplacer
  {
    /// css class name -> font name (guid.ttf)
    private Dictionary<string, string> _classToFont = new();

    /// font-family name -> font name (guid.ttf)
    private Dictionary<string, string> _fonts = new();

    /// font name (guid.ttf) -> Converter
    private Dictionary<string, Func<char, char>> _fontToConverter = new();

    private IFontProvider _fontProvider;

    public FontReplacer(IFontProvider fontProvider)
    {
      _fontProvider = fontProvider;
    }

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

    private static object CacheKey = new();
    private Func<char, char> ReadFont(string fontValue)
    {
      var converter = (ConcurrentDictionary<string, Func<char, char>>) 
        _fontProvider.Cache.GetOrAdd(CacheKey, x => new ConcurrentDictionary<string, Func<char, char>>());

      if (converter.TryGetValue(fontValue, out var translator))
      {
        return translator;
      }

      var izb = new OpenFontReader().Read(new MemoryStream(_fontProvider.GetFont(fontValue, "fonts1").Result));
      var ptsans = new OpenFontReader().Read(new MemoryStream(_fontProvider.GetFont("pt-sans-v12-latin_cyrillic-regular.ttf", "fonts").Result));

      List<char> alphabet = new();
      for (char l = 'A'; l <= 'Z'; l++) alphabet.Add(l);
      for (char l = 'z'; l <= 'z'; l++) alphabet.Add(l);
      for (char l = '0'; l <= '9'; l++) alphabet.Add(l);
      for (char l = 'а'; l <= 'я'; l++) alphabet.Add(l);
      for (char l = 'А'; l <= 'Я'; l++) alphabet.Add(l);

      var glyphIndexToLetter = new char[ptsans.GlyphCount];
      foreach (var c in alphabet)
        glyphIndexToLetter[ptsans.GetGlyphIndex(c)] = c;

      translator = c => glyphIndexToLetter[izb.GetGlyphIndex(c)];
      converter[fontValue] = translator;
      return translator;
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