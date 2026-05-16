namespace CutBookApi.Models;

public class QueueEntry
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int HaircutRequestId { get; set; }
    public int CustomerId { get; set; }
    public int Position { get; set; }
    public int TokenNumber { get; set; }
    public string Status { get; set; } = "Waiting"; // Waiting | InProgress | Done
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Shop Shop { get; set; } = null!;
    public HaircutRequest HaircutRequest { get; set; } = null!;
    public User Customer { get; set; } = null!;
}
