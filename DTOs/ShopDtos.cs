namespace CutBook.API.DTOs;

public class CreateShopDto
{
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int AvgServiceTime { get; set; } = 15;
}

public class ShopResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string PlanName { get; set; } = "Free";
    public int TotalWaiting { get; set; }
}
