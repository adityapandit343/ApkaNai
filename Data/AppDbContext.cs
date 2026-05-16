using Microsoft.EntityFrameworkCore;
using CutBookApi.Models;

namespace CutBookApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<ShopService> ShopServices => Set<ShopService>();
    public DbSet<HaircutRequest> HaircutRequests => Set<HaircutRequest>();
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        base.OnModelCreating(modelBuilder);

      
    }

}
