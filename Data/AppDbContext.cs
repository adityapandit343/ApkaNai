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
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("Customer");
        });

        // Shop
        modelBuilder.Entity<Shop>(e =>
        {
            e.HasOne(s => s.Owner)
             .WithOne(u => u.Shop)
             .HasForeignKey<Shop>(s => s.OwnerId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(s => s.SalonType).HasDefaultValue("Unisex");
        });

        // ShopService
        modelBuilder.Entity<ShopService>(e =>
        {
            e.HasOne(ss => ss.Shop)
             .WithMany(s => s.Services)
             .HasForeignKey(ss => ss.ShopId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // HaircutRequest
        modelBuilder.Entity<HaircutRequest>(e =>
        {
            e.HasOne(r => r.Customer)
             .WithMany(u => u.HaircutRequests)
             .HasForeignKey(r => r.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Shop)
             .WithMany(s => s.HaircutRequests)
             .HasForeignKey(r => r.ShopId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // QueueEntry
        modelBuilder.Entity<QueueEntry>(e =>
        {
            e.HasOne(q => q.Shop)
             .WithMany(s => s.Queue)
             .HasForeignKey(q => q.ShopId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(q => q.HaircutRequest)
             .WithOne(r => r.QueueEntry)
             .HasForeignKey<QueueEntry>(q => q.HaircutRequestId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(q => q.Customer)
             .WithMany()
             .HasForeignKey(q => q.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
