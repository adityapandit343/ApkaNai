namespace CutBook.API.Models;

public class Subscription
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RazorpaySubscriptionId { get; set; }
    public string? RazorpayPaymentId { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
}
