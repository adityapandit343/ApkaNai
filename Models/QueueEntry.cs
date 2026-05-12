namespace CutBook.API.Models;

public class QueueEntry
{
    public int Id { get; set; }
    public int TokenNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public QueueStatus Status { get; set; } = QueueStatus.Waiting;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CalledAt { get; set; }
    public DateTime? ServedAt { get; set; }


    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
}

public enum QueueStatus
{
    Waiting,
    Called,
    Serving,
    Done,
    NoShow
}
