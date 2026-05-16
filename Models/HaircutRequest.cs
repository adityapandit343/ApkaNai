namespace CutBookApi.Models;

public class HaircutRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ShopId { get; set; }
    public string RequestedServices { get; set; } = string.Empty; // comma-separated service IDs
    public string HairStyle { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending | Accepted | Rejected | InQueue | Completed | Cancelled
    public int? TokenNumber { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public User Customer { get; set; } = null!;
    public Shop Shop { get; set; } = null!;
    public QueueEntry? QueueEntry { get; set; }
}
