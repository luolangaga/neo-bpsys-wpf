using Microsoft.EntityFrameworkCore;
using PluginMarket.Api.Models;

namespace PluginMarket.Api.Data;

public class PluginDbContext(DbContextOptions<PluginDbContext> options) : DbContext(options)
{
    public DbSet<PluginPackage> PluginPackages => Set<PluginPackage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PluginPackage>()
            .HasIndex(x => new { x.Slug, x.Version })
            .IsUnique();
    }
}
