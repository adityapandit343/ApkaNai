namespace CutBook.API.Models;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;        // Free, Pro, Premium
    public decimal Price { get; set; }                       // 0, 299, 599
    public int MaxBookingsPerMonth { get; set; }             // 20, unlimited=-1
    public bool WhatsAppAlerts { get; set; } = false;
    public bool Analytics { get; set; } = false;
    public bool MultipleStaff { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<Shop> Shops { get; set; } = new List<Shop>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
