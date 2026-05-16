using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CutBookApi.Data;
using CutBookApi.DTOs;
using CutBookApi.Hubs;
using CutBookApi.Models;

namespace CutBookApi.Services;

public interface IQueueService
{
    Task<HaircutRequestResponseDto> CreateRequestAsync(int customerId, CreateHaircutRequestDto dto);
    Task<HaircutRequestResponseDto> AcceptRequestAsync(int ownerId, int requestId);
    Task<HaircutRequestResponseDto> RejectRequestAsync(int ownerId, int requestId);
    Task<List<QueueEntryResponseDto>> GetQueueAsync(int ownerId);
    Task<QueueEntryResponseDto> NextCustomerAsync(int ownerId);
    Task<List<HaircutRequestResponseDto>> GetPendingRequestsAsync(int ownerId);
    Task<HaircutRequestResponseDto> GetMyRequestAsync(int customerId);
}

public class QueueService : IQueueService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ShopHub> _hub;

    public QueueService(AppDbContext db, IHubContext<ShopHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<HaircutRequestResponseDto> CreateRequestAsync(int customerId, CreateHaircutRequestDto dto)
    {
        var shop = await _db.Shops.Include(s => s.Services).FirstOrDefaultAsync(s => s.Id == dto.ShopId && s.IsLive)
            ?? throw new KeyNotFoundException("Shop not found or not live.");

        // Validate services belong to this shop
        var serviceIds = dto.ServiceIds;
        var services = await _db.ShopServices
            .Where(s => serviceIds.Contains(s.Id) && s.ShopId == dto.ShopId)
            .ToListAsync();

        if (services.Count != serviceIds.Count)
            throw new InvalidOperationException("One or more services not found for this shop.");

        var request = new HaircutRequest
        {
            CustomerId = customerId,
            ShopId = dto.ShopId,
            RequestedServices = string.Join(",", dto.ServiceIds),
            HairStyle = dto.HairStyle,
            Notes = dto.Notes,
            Status = "Pending"
        };

        _db.HaircutRequests.Add(request);
        await _db.SaveChangesAsync();

        var responseDto = await BuildRequestResponseAsync(request);

        // Notify shop owner in realtime
        await _hub.Clients.Group($"shop_{dto.ShopId}").SendAsync("NewRequest", responseDto);

        return responseDto;
    }

    public async Task<HaircutRequestResponseDto> AcceptRequestAsync(int ownerId, int requestId)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId)
            ?? throw new KeyNotFoundException("Shop not found.");

        var request = await _db.HaircutRequests
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ShopId == shop.Id && r.Status == "Pending")
            ?? throw new KeyNotFoundException("Request not found or already processed.");

        // Generate token
        var lastToken = await _db.QueueEntries
            .Where(q => q.ShopId == shop.Id)
            .OrderByDescending(q => q.TokenNumber)
            .Select(q => (int?)q.TokenNumber)
            .FirstOrDefaultAsync() ?? 0;

        var position = await _db.QueueEntries
            .CountAsync(q => q.ShopId == shop.Id && q.Status != "Done") + 1;

        var token = lastToken + 1;

        request.Status = "Accepted";
        request.AcceptedAt = DateTime.UtcNow;
        request.TokenNumber = token;

        var queueEntry = new QueueEntry
        {
            ShopId = shop.Id,
            HaircutRequestId = request.Id,
            CustomerId = request.CustomerId,
            Position = position,
            TokenNumber = token,
            Status = position == 1 ? "InProgress" : "Waiting"
        };

        if (position == 1) queueEntry.StartedAt = DateTime.UtcNow;

        _db.QueueEntries.Add(queueEntry);
        await _db.SaveChangesAsync();

        var responseDto = await BuildRequestResponseAsync(request);

        // Notify customer in realtime
        await _hub.Clients.Group($"customer_{request.CustomerId}").SendAsync("RequestAccepted", responseDto);

        // Broadcast updated queue to shop
        await BroadcastQueueAsync(shop.Id);

        return responseDto;
    }

    public async Task<HaircutRequestResponseDto> RejectRequestAsync(int ownerId, int requestId)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId)
            ?? throw new KeyNotFoundException("Shop not found.");

        var request = await _db.HaircutRequests
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ShopId == shop.Id && r.Status == "Pending")
            ?? throw new KeyNotFoundException("Request not found or already processed.");

        request.Status = "Rejected";
        await _db.SaveChangesAsync();

        var responseDto = await BuildRequestResponseAsync(request);

        // Notify customer
        await _hub.Clients.Group($"customer_{request.CustomerId}").SendAsync("RequestRejected", responseDto);

        return responseDto;
    }

    public async Task<List<QueueEntryResponseDto>> GetQueueAsync(int ownerId)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId)
            ?? throw new KeyNotFoundException("Shop not found.");

        var entries = await _db.QueueEntries
            .Include(q => q.Customer)
            .Include(q => q.HaircutRequest)
            .Where(q => q.ShopId == shop.Id && q.Status != "Done")
            .OrderBy(q => q.Position)
            .ToListAsync();

        return await BuildQueueResponseAsync(entries);
    }

    public async Task<QueueEntryResponseDto> NextCustomerAsync(int ownerId)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId)
            ?? throw new KeyNotFoundException("Shop not found.");

        // Mark current InProgress as Done
        var current = await _db.QueueEntries
            .Include(q => q.HaircutRequest)
            .FirstOrDefaultAsync(q => q.ShopId == shop.Id && q.Status == "InProgress");

        if (current != null)
        {
            current.Status = "Done";
            current.CompletedAt = DateTime.UtcNow;
            current.HaircutRequest.Status = "Completed";
            current.HaircutRequest.CompletedAt = DateTime.UtcNow;

            // Notify customer their haircut is done
            await _hub.Clients.Group($"customer_{current.CustomerId}").SendAsync("HaircutCompleted", new { TokenNumber = current.TokenNumber });
        }

        // Move next waiting customer to InProgress
        var next = await _db.QueueEntries
            .Include(q => q.Customer)
            .Include(q => q.HaircutRequest)
            .Where(q => q.ShopId == shop.Id && q.Status == "Waiting")
            .OrderBy(q => q.Position)
            .FirstOrDefaultAsync();

        if (next != null)
        {
            next.Status = "InProgress";
            next.StartedAt = DateTime.UtcNow;
            await _hub.Clients.Group($"customer_{next.CustomerId}").SendAsync("YourTurn", new { TokenNumber = next.TokenNumber });
        }

        await _db.SaveChangesAsync();

        // Recalculate positions
        await RecalculatePositionsAsync(shop.Id);
        await BroadcastQueueAsync(shop.Id);

        if (next == null) throw new InvalidOperationException("Queue is empty.");

        var dto = (await BuildQueueResponseAsync(new List<QueueEntry> { next })).First();
        return dto;
    }

    public async Task<List<HaircutRequestResponseDto>> GetPendingRequestsAsync(int ownerId)
    {
        var shop = await _db.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId)
            ?? throw new KeyNotFoundException("Shop not found.");

        var requests = await _db.HaircutRequests
            .Include(r => r.Customer)
            .Where(r => r.ShopId == shop.Id && r.Status == "Pending")
            .OrderBy(r => r.RequestedAt)
            .ToListAsync();

        var result = new List<HaircutRequestResponseDto>();
        foreach (var r in requests)
            result.Add(await BuildRequestResponseAsync(r));

        return result;
    }

    public async Task<HaircutRequestResponseDto> GetMyRequestAsync(int customerId)
    {
        var request = await _db.HaircutRequests
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId && (r.Status == "Pending" || r.Status == "Accepted" || r.Status == "InQueue"))
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("No active request found.");

        return await BuildRequestResponseAsync(request);
    }

    // ---------- helpers ----------

    private async Task<HaircutRequestResponseDto> BuildRequestResponseAsync(HaircutRequest request)
    {
        var customer = request.Customer ?? await _db.Users.FindAsync(request.CustomerId);
        var serviceIds = request.RequestedServices.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse).ToList();
        var services = await _db.ShopServices.Where(s => serviceIds.Contains(s.Id)).ToListAsync();

        return new HaircutRequestResponseDto
        {
            Id = request.Id,
            CustomerId = request.CustomerId,
            CustomerName = customer?.FullName ?? string.Empty,
            CustomerPhone = customer?.PhoneNumber ?? string.Empty,
            ShopId = request.ShopId,
            HairStyle = request.HairStyle,
            Notes = request.Notes,
            Status = request.Status,
            TokenNumber = request.TokenNumber,
            RequestedAt = request.RequestedAt,
            RequestedServiceNames = services.Select(s => s.ServiceName).ToList(),
            TotalEstimatedMinutes = services.Sum(s => s.EstimatedMinutes)
        };
    }

    private async Task<List<QueueEntryResponseDto>> BuildQueueResponseAsync(List<QueueEntry> entries)
    {
        var result = new List<QueueEntryResponseDto>();
        foreach (var entry in entries)
        {
            var customer = entry.Customer ?? await _db.Users.FindAsync(entry.CustomerId);
            var req = entry.HaircutRequest ?? await _db.HaircutRequests.FindAsync(entry.HaircutRequestId);
            var serviceIds = req?.RequestedServices.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToList() ?? new();
            var services = await _db.ShopServices.Where(s => serviceIds.Contains(s.Id)).ToListAsync();

            result.Add(new QueueEntryResponseDto
            {
                Id = entry.Id,
                Position = entry.Position,
                TokenNumber = entry.TokenNumber,
                CustomerName = customer?.FullName ?? string.Empty,
                CustomerPhone = customer?.PhoneNumber ?? string.Empty,
                Status = entry.Status,
                HairStyle = req?.HairStyle ?? string.Empty,
                Services = services.Select(s => s.ServiceName).ToList(),
                TotalEstimatedMinutes = services.Sum(s => s.EstimatedMinutes),
                EnteredAt = entry.EnteredAt
            });
        }
        return result;
    }

    private async Task RecalculatePositionsAsync(int shopId)
    {
        var waiting = await _db.QueueEntries
            .Where(q => q.ShopId == shopId && q.Status == "Waiting")
            .OrderBy(q => q.Position)
            .ToListAsync();

        for (int i = 0; i < waiting.Count; i++)
            waiting[i].Position = i + 2; // InProgress is position 1

        await _db.SaveChangesAsync();
    }

    private async Task BroadcastQueueAsync(int shopId)
    {
        var entries = await _db.QueueEntries
            .Include(q => q.Customer)
            .Include(q => q.HaircutRequest)
            .Where(q => q.ShopId == shopId && q.Status != "Done")
            .OrderBy(q => q.Position)
            .ToListAsync();

        var queueDto = await BuildQueueResponseAsync(entries);
        await _hub.Clients.Group($"shop_{shopId}").SendAsync("QueueUpdated", queueDto);
    }
}
