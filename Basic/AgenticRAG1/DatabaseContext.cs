using AgenticRAG1.Models;
using Microsoft.EntityFrameworkCore;

namespace AgenticRAG1;

public class DatabaseContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=articles.db");
    }
}
