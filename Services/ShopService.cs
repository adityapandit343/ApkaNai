using Microsoft.EntityFrameworkCore;
using CutBookApi.Data;
using CutBookApi.DTOs;
using CutBookApi.Helpers;
using CutBookApi.Models;

namespace CutBookApi.Services;

public interface IShopService
{
    Task<ShopResponseDto> CreateShopAsync(int ownerId, CreateShopDto dto);
    Task<ShopResponseDto> UpdateShopAsync(int ownerId, UpdateShopDto dto);
    Task<ShopResponseDto> GoLiveAsync(int ownerId, GoLiveDto dto);
    Task GoOfflineAsync(int ownerId);
    Task<ShopResponseDto> GetMyShopAsync(int ownerId);
    Task<List<ShopResponseDto>> SearchNearbyShopsAsync(SearchShopDto dto);
    Task<ServiceResponseDto> AddServiceAsync(int ownerId, CreateServiceDto dto);
    Task DeleteServiceAsync(int ownerId, int serviceId);
}

public class ShopService : IShopService
{
    private readonly AppDbContext _db;

    public ShopService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ShopResponseDto> CreateShopAsync(int ownerId, CreateShopDto dto)
    {
        if (await _db.Shops.AnyAsync(s => s.OwnerId == ownerId))
            throw new InvalidOperationException("You already have a shop.");

        var shop = new Shop
        {
            OwnerId = ownerId,
            ShopName = dto.ShopName,
            Description = dto.Description,
            Address = dto.Address,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            SalonType = dto.SalonType,
            PhoneNumber = dto.PhoneNumber,
            OpeningTime = dto.OpeningTime,
            ClosingTime = dto.ClosingTime,
            TrialEndsAt = DateTime.UtcNow.AddMonths(1),
            IsLive = false
        };

        foreach (var svc in dto.Services)
        {
            shop.Services.Add(new Models.ShopService
            {
                ServiceName = svc.ServiceName,
                Category = svc.Category,
                EstimatedMinutes = svc.EstimatedMinutes,
                Price = svc.Price
            });
        }

        _db.Shops.Add(shop);
        await _db.SaveChangesAsync();

        return await ToResponseDto(shop, null, null);
    }

    public async Task<ShopResponseDto> UpdateShopAsync(int ownerId, UpdateShopDto dto)
    {
        var shop = await GetOwnerShopAsync(ownerId);

        if (dto.ShopName != null) shop.ShopName = dto.ShopName;
        if (dto.Description != null) shop.Description = dto.Description;
        if (dto.Address != null) shop.Address = dto.Address;
        if (dto.Latitude.HasValue) shop.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue) shop.Longitude = dto.Longitude.Value;
        if (dto.SalonType != null) shop.SalonType = dto.SalonType;
        if (dto.PhoneNumber != null) shop.PhoneNumber = dto.PhoneNumber;
        if (dto.OpeningTime.HasValue) shop.OpeningTime = dto.OpeningTime.Value;
        if (dto.ClosingTime.HasValue) shop.ClosingTime = dto.ClosingTime.Value;

        await _db.SaveChangesAsync();
        return await ToResponseDto(shop, null, null);
    }

    public async Task<ShopResponseDto> GoLiveAsync(int ownerId, GoLiveDto dto)
    {
        var shop = await GetOwnerShopAsync(ownerId);

        if (dto.Latitude == 0 || dto.Longitude == 0)
            throw new InvalidOperationException("Valid latitude and longitude are required to go live.");

        if (shop.TrialEndsAt < DateTime.UtcNow)
            throw new InvalidOperationException("Your free trial has expired. Please subscribe to continue.");

        shop.Latitude = dto.Latitude;
        shop.Longitude = dto.Longitude;
        shop.IsLive = true;
        await _db.SaveChangesAsync();

        return await ToResponseDto(shop, null, null);
    }

    public async Task GoOfflineAsync(int ownerId)
    {
        var shop = await GetOwnerShopAsync(ownerId);
        shop.IsLive = false;
        await _db.SaveChangesAsync();
    }

    public async Task<ShopResponseDto> GetMyShopAsync(int ownerId)
    {
        var shop = await GetOwnerShopAsync(ownerId);
        return await ToResponseDto(shop, null, null);
    }

    public async Task<List<ShopResponseDto>> SearchNearbyShopsAsync(SearchShopDto dto)
    {
        var shops = await _db.Shops
            .Include(s => s.Services)
            .Include(s => s.Queue)
                .ThenInclude(q => q.HaircutRequest)
            .Where(s => s.IsActive)
            .ToListAsync();

        var results = new List<(Shop shop, double dist, int estimatedMinutes)>();

        foreach (var shop in shops)
        {
            var dist = GeoHelper.HaversineDistance(
                dto.Latitude,
                dto.Longitude,
                shop.Latitude,
                shop.Longitude
            );

            if (dist > dto.RadiusKm)
                continue;

            if (!string.IsNullOrEmpty(dto.SalonType) &&
                !shop.SalonType.Equals(dto.SalonType, StringComparison.OrdinalIgnoreCase))
                continue;

            int estimatedMinutes = 0;

            foreach (var q in shop.Queue.Where(x => x.Status != "Done"))
            {
                var req = q.HaircutRequest;
                if (req == null) continue;

                if (!string.IsNullOrEmpty(req.RequestedServices))
                {
                    var ids = req.RequestedServices
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x, out var id) ? id : 0)
                        .Where(x => x > 0);

                    estimatedMinutes += shop.Services
                        .Where(s => ids.Contains(s.Id))
                        .Sum(s => s.EstimatedMinutes);
                }
                else
                {
                    // fallback: assume full haircut service
                    estimatedMinutes += shop.Services
                        .Where(s => s.ServiceName == "Haircut")
                        .Sum(s => s.EstimatedMinutes);
                }
            }

            results.Add((shop, dist, estimatedMinutes));
        }

        var response = new List<ShopResponseDto>();

        foreach (var item in results.OrderBy(x => x.dist))
        {
            response.Add(await ToResponseDto(
                item.shop,
                item.dist,
                item.estimatedMinutes
            ));
        }

        return response;
    }

    public async Task<ServiceResponseDto> AddServiceAsync(int ownerId, CreateServiceDto dto)
    {
        var shop = await GetOwnerShopAsync(ownerId);
        var svc = new Models.ShopService
        {
            ShopId = shop.Id,
            ServiceName = dto.ServiceName,
            Category = dto.Category,
            EstimatedMinutes = dto.EstimatedMinutes,
            Price = dto.Price
        };
        _db.ShopServices.Add(svc);
        await _db.SaveChangesAsync();

        return new ServiceResponseDto
        {
            Id = svc.Id,
            ServiceName = svc.ServiceName,
            Category = svc.Category,
            EstimatedMinutes = svc.EstimatedMinutes,
            Price = svc.Price,
            IsAvailable = svc.IsAvailable
        };
    }

    public async Task DeleteServiceAsync(int ownerId, int serviceId)
    {
        var shop = await GetOwnerShopAsync(ownerId);
        var svc = await _db.ShopServices.FirstOrDefaultAsync(s => s.Id == serviceId && s.ShopId == shop.Id)
            ?? throw new KeyNotFoundException("Service not found.");
        _db.ShopServices.Remove(svc);
        await _db.SaveChangesAsync();
    }

    // ---------- helpers ----------

    private async Task<Shop> GetOwnerShopAsync(int ownerId)
    {
        return await _db.Shops
            .Include(s => s.Services)
            .Include(s => s.Owner)
            .Include(s => s.Queue)
            .FirstOrDefaultAsync(s => s.OwnerId == ownerId)
            ?? throw new KeyNotFoundException("Shop not found.");
    }

    private Task<ShopResponseDto> ToResponseDto(Shop shop, double? distKm, object? _)
    {
        var activeQueueCount = shop.Queue?.Count(q => q.Status != "Done") ?? 0;
        var dto = new ShopResponseDto
        {
            Id = shop.Id,
            ShopName = shop.ShopName,
            Description = shop.Description,
            Address = shop.Address,
            Latitude = shop.Latitude,
            Longitude = shop.Longitude,
            SalonType = shop.SalonType,
            PhoneNumber = shop.PhoneNumber,
            OpeningTime = shop.OpeningTime,
            ClosingTime = shop.ClosingTime,
            IsLive = shop.IsLive,
            TrialEndsAt = shop.TrialEndsAt,
            DistanceKm = distKm.HasValue ? Math.Round(distKm.Value, 2) : null,
            OwnerName = shop.Owner?.FullName ?? string.Empty,
            ActiveQueueCount = activeQueueCount,
            Services = shop.Services?.Select(s => new ServiceResponseDto
            {
                Id = s.Id,
                ServiceName = s.ServiceName,
                Category = s.Category,
                EstimatedMinutes = s.EstimatedMinutes,
                Price = s.Price,
                IsAvailable = s.IsAvailable
            }).ToList() ?? new()
        };
        return Task.FromResult(dto);
    }
}
