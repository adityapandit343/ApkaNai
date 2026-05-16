namespace CutBookApi.Models;

public class Shop
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SalonType { get; set; } = "Unisex"; // Male | Female | Unisex
    public string PhoneNumber { get; set; } = string.Empty;
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsLive { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime TrialEndsAt { get; set; }

    // Navigation
    public User Owner { get; set; } = null!;
    public ICollection<ShopService> Services { get; set; } = new List<ShopService>();
    public ICollection<HaircutRequest> HaircutRequests { get; set; } = new List<HaircutRequest>();
    public ICollection<QueueEntry> Queue { get; set; } = new List<QueueEntry>();
}
