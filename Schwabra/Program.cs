using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Schwabra
{
  public class Program
  {
    static int Main(string[] args)
    {
      if (args.Length != 1)
      {
        Console.WriteLine("use:");
        Console.WriteLine("    dotnet run <saved html page> ");
        Console.WriteLine("    dotnet run <directory with *.html>");

        return -1;
      }

      var inputArg = args[0];
      using var dbContext = new ElectionContext();
      var processedStations = new ConcurrentBag<Station>();
      if (Directory.Exists(inputArg))
      {
        // batch processing
        var directory = Directory.GetFiles(inputArg);
        Parallel.For(0, directory.Length, i =>
        {
          var file = directory[i];
          try
          {
            Console.WriteLine($"Processing {file} ({i} from {directory.Length})");
            processedStations.Add(Extract(file));
          }
          catch (Exception)
          {
            Console.Error.WriteLine($"Error processing file: {file}");
            throw;
          }
        });
        dbContext.station.AddRange(processedStations);
      }
      else
      {
        dbContext.Add(Extract(inputArg));
      }

      Console.WriteLine("Saving changes to sqlite...");
      dbContext.SaveChanges();


      return 0;
    }

    private static Station Extract(string inputPath)
    {
      var inputText = File.ReadAllText(inputPath);
      var doc = new HtmlDocument();
      doc.LoadHtml(inputText);

      string name = ExtractName(doc);
      string path = ExtractPath(doc);
      var rows = ExtractRows(doc).ToArray();
      var station = new Station()
      {
        filename = inputPath,
        name = name,
        path = path
      };
      station.rows.AddRange(rows);

      return station;
    }

    public static string ExtractName(HtmlDocument doc)
    {
      foreach (var td in doc.DocumentNode.SelectNodes("//td") ?? Enumerable.Empty<HtmlNode>())
      {
        if (td.InnerText.Contains("Наименование избирательной комиссии"))
        {
          return td.ParentNode.Elements("td").Last().InnerText;
        }
      }

      return null;
    }

    public static string ExtractPath(HtmlDocument doc)
    {
      var result = new List<string>();
      foreach (var node in doc.DocumentNode.SelectSingleNode("//ul[@class='breadcrumb']")?.SelectNodes("li") ?? Enumerable.Empty<HtmlNode>())
      {
        var val = node.InnerText.Trim();
        if (val != "menu")
          result.Add(val);
      }

      return string.Join(";", result);
    }

    public static IEnumerable<Result> ExtractRows(HtmlDocument doc)
    {
      var mainTable = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'table-sm')]");
      foreach (var tr in mainTable?.SelectNodes("tr") ?? Enumerable.Empty<HtmlNode>())
      {
        if (tr.InnerText.Trim() == "") continue;
        var tds = tr.Elements("td").ToArray();
        var numNode = tds[0];
        var titleNode = tds[1];
        var valNode = tds[2];

        var num = int.Parse(numNode.InnerText);
        var title = titleNode.InnerText;
        double? percent = null;
        if (!int.TryParse(valNode.InnerText, out var value))
        {
          var nums = valNode.InnerText.Split(new []{' ', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
          value = int.Parse(nums[0]);
          percent = double.Parse(nums[1].TrimEnd('%'), CultureInfo.InvariantCulture);
        }
        
        yield return new Result(){ num = num, title = title, value = value, value_percent = percent};
      }
    }
  }
}