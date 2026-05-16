namespace CutBookApi.DTOs;

public class CreateShopDto
{
    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SalonType { get; set; } = "Unisex"; // Male | Female | Unisex
    public string PhoneNumber { get; set; } = string.Empty;
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public List<CreateServiceDto> Services { get; set; } = new();
}

public class UpdateShopDto
{
    public string? ShopName { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? SalonType { get; set; }
    public string? PhoneNumber { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
}

public class CreateServiceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public decimal Price { get; set; }
}

public class ShopResponseDto
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SalonType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public bool IsLive { get; set; }
    public DateTime TrialEndsAt { get; set; }
    public double? DistanceKm { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public List<ServiceResponseDto> Services { get; set; } = new();
    public int ActiveQueueCount { get; set; }
}

public class ServiceResponseDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class GoLiveDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
