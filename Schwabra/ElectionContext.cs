using Microsoft.EntityFrameworkCore;

namespace Schwabra
{
  public class ElectionContext : DbContext
  {
    public DbSet<Station> station { get; set; }
    public DbSet<Result> result { get; set; }

    public string DbPath { get; }

    public ElectionContext() : this("election.sqlite3")
    {
    }

    public ElectionContext(string path)
    {
      DbPath = path;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
      => options.UseSqlite($"Data Source={DbPath}");
  }
}