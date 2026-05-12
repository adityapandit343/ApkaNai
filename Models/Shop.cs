namespace CutBook.API.Models;

public class Shop
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public int? PlanId { get; set; }
    public Plan? Plan { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int AvgServiceTime { get; set; } = 15;

    public ICollection<QueueEntry> QueueEntries { get; set; } = new List<QueueEntry>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
