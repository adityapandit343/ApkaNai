using Microsoft.EntityFrameworkCore;
using CutBook.API.Models;

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
        // User → Shops (one to many)
        modelBuilder.Entity<Shop>()
            .HasOne(s => s.Owner)
            .WithMany(u => u.Shops)
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Shop → QueueEntries
        modelBuilder.Entity<QueueEntry>()
            .HasOne(q => q.Shop)
            .WithMany(s => s.QueueEntries)
            .HasForeignKey(q => q.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        // Shop → Subscription
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Shop)
            .WithMany(sh => sh.Subscriptions)
            .HasForeignKey(s => s.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plan → Subscription
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision
        modelBuilder.Entity<Plan>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        // Seed default plans
        modelBuilder.Entity<Plan>().HasData(
            new Plan { Id = 1, Name = "Free",    Price = 0,   MaxBookingsPerMonth = 20,  WhatsAppAlerts = false, Analytics = false, MultipleStaff = false },
            new Plan { Id = 2, Name = "Pro",     Price = 299, MaxBookingsPerMonth = -1,  WhatsAppAlerts = true,  Analytics = false, MultipleStaff = false },
            new Plan { Id = 3, Name = "Premium", Price = 599, MaxBookingsPerMonth = -1,  WhatsAppAlerts = true,  Analytics = true,  MultipleStaff = true  }
        );
    }
}
