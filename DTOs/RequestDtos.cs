namespace CutBookApi.DTOs;

public class SearchShopDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 5;
    public string? SalonType { get; set; } // optional filter
}

public class CreateHaircutRequestDto
{
    public int ShopId { get; set; }
    public List<int> ServiceIds { get; set; } = new();
    public string HairStyle { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class HaircutRequestResponseDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int ShopId { get; set; }
    public string HairStyle { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? TokenNumber { get; set; }
    public DateTime RequestedAt { get; set; }
    public List<string> RequestedServiceNames { get; set; } = new();
    public int TotalEstimatedMinutes { get; set; }
}

public class AcceptRequestDto
{
    public int RequestId { get; set; }
}

public class QueueEntryResponseDto
{
    public int Id { get; set; }
    public int Position { get; set; }
    public int TokenNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HairStyle { get; set; } = string.Empty;
    public List<string> Services { get; set; } = new();
    public int TotalEstimatedMinutes { get; set; }
    public DateTime EnteredAt { get; set; }
}
