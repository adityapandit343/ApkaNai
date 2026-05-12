
using CutBook.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CutBook.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Shop -> Owner (User) Relationship
        modelBuilder.Entity<Shop>()
            .HasOne(s => s.Owner)
            .WithMany() // User can have many shops
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Shop -> Plan Relationship
        modelBuilder.Entity<Shop>()
            .HasOne(s => s.Plan)
            .WithMany(p => p.Shops)
            .HasForeignKey(s => s.PlanId);

        // QueueEntry -> Shop Relationship
        modelBuilder.Entity<QueueEntry>()
            .HasOne(q => q.Shop)
            .WithMany(s => s.QueueEntries)
            .HasForeignKey(q => q.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        // Subscription Relationships
        modelBuilder.Entity<Subscription>()
            .HasOne(sub => sub.Shop)
            .WithMany(s => s.Subscriptions)
            .HasForeignKey(sub => sub.ShopId);

        modelBuilder.Entity<Subscription>()
            .HasOne(sub => sub.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(sub => sub.PlanId);

        // Decimal precision for Price in Plan
        modelBuilder.Entity<Plan>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
    }
}