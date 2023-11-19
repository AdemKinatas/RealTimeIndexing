using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using RealTimeIndexing.Entities;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;

public class NorthwindContext : DbContext
{
    public NorthwindContext(DbContextOptions<NorthwindContext> options)
        : base(options)
    {

    }

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Change Tracking'i etkinleştir
        // optionsBuilder.EnableSensitiveDataLogging();
    }
}
