using System.Collections.Generic;

namespace Schwabra
{
  public class Station
  {
    public int id { get; set; }
    public string name { get; set; }
    public string filename { get; set; }
    public string path { get; set; }
    public List<Result> rows { get; set; } = new();
  }
}