namespace CutBookApi.Models;

public class ShopService
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public string ServiceName { get; set; } = string.Empty; // Haircut, Shave, Color, etc.
    public string Category { get; set; } = string.Empty;    // Hair | Beard | Skin | Other
    public int EstimatedMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public Shop Shop { get; set; } = null!;
}
