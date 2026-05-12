using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CutBook.API.Data;
using CutBook.API.DTOs;
using CutBook.API.Hubs;
using CutBook.API.Models;

namespace CutBook.API.Services;

public class QueueService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<QueueHub> _hub;

    public QueueService(AppDbContext db, IHubContext<QueueHub> hub)
    {
        _db  = db;
        _hub = hub;
    }

    // Customer queue mein join karta hai
    public async Task<QueueEntryResponseDto?> JoinQueueAsync(int shopId, JoinQueueDto dto)
    {
        var shop = await _db.Shops.FindAsync(shopId);
        if (shop == null || !shop.IsActive) return null;

        // Next token number nikalo
        var lastToken = await _db.QueueEntries
            .Where(q => q.ShopId == shopId && q.JoinedAt.Date == DateTime.UtcNow.Date)
            .MaxAsync(q => (int?)q.TokenNumber) ?? 0;

        var entry = new QueueEntry
        {
            ShopId       = shopId,
            CustomerName = dto.CustomerName,
            PhoneNumber  = dto.PhoneNumber,
            TokenNumber  = lastToken + 1,
            Status       = QueueStatus.Waiting
        };

        _db.QueueEntries.Add(entry);
        await _db.SaveChangesAsync();

        // Sab ko real-time update bhejo
        await BroadcastQueueUpdate(shopId);

        var position = await GetPositionAsync(shopId, entry.Id);
        return MapToDto(entry, position);
    }

    // Barber next customer bulata hai
    public async Task<QueueEntryResponseDto?> CallNextAsync(int shopId)
    {
        var next = await _db.QueueEntries
            .Where(q => q.ShopId == shopId && q.Status == QueueStatus.Waiting)
            .OrderBy(q => q.TokenNumber)
            .FirstOrDefaultAsync();

        if (next == null) return null;

        next.Status   = QueueStatus.Called;
        next.CalledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await BroadcastQueueUpdate(shopId);
        return MapToDto(next, 0);
    }

    // Barber serving mark karta hai
    public async Task<bool> MarkServingAsync(int entryId)
    {
        var entry = await _db.QueueEntries.FindAsync(entryId);
        if (entry == null) return false;

        entry.Status = QueueStatus.Serving;
        await _db.SaveChangesAsync();
        await BroadcastQueueUpdate(entry.ShopId);
        return true;
    }

    // Done mark karo
    public async Task<bool> MarkDoneAsync(int entryId)
    {
        var entry = await _db.QueueEntries.FindAsync(entryId);
        if (entry == null) return false;

        entry.Status   = QueueStatus.Done;
        entry.ServedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await BroadcastQueueUpdate(entry.ShopId);
        return true;
    }

    // No show mark karo
    public async Task<bool> MarkNoShowAsync(int entryId)
    {
        var entry = await _db.QueueEntries.FindAsync(entryId);
        if (entry == null) return false;

        entry.Status = QueueStatus.NoShow;
        await _db.SaveChangesAsync();
        await BroadcastQueueUpdate(entry.ShopId);
        return true;
    }

    // Queue ka poora status
    public async Task<QueueStatusDto> GetQueueStatusAsync(int shopId)
    {
        var entries = await _db.QueueEntries
            .Where(q => q.ShopId == shopId
                     && q.JoinedAt.Date == DateTime.UtcNow.Date
                     && (q.Status == QueueStatus.Waiting || q.Status == QueueStatus.Called || q.Status == QueueStatus.Serving))
            .OrderBy(q => q.TokenNumber)
            .ToListAsync();

        var currentToken = entries
            .Where(e => e.Status == QueueStatus.Serving || e.Status == QueueStatus.Called)
            .Select(e => e.TokenNumber)
            .FirstOrDefault();

        var waitingList = entries
            .Where(e => e.Status == QueueStatus.Waiting)
            .Select((e, i) => MapToDto(e, i + 1))
            .ToList();

        return new QueueStatusDto
        {
            TotalWaiting = waitingList.Count,
            CurrentToken = currentToken,
            WaitingList  = waitingList
        };
    }

    // SignalR broadcast — sab phones update ho jaate hain
    private async Task BroadcastQueueUpdate(int shopId)
    {
        var status = await GetQueueStatusAsync(shopId);
        await _hub.Clients.Group($"shop-{shopId}").SendAsync("QueueUpdated", status);
    }

    private async Task<int> GetPositionAsync(int shopId, int entryId)
    {
        var waiting = await _db.QueueEntries
            .Where(q => q.ShopId == shopId && q.Status == QueueStatus.Waiting)
            .OrderBy(q => q.TokenNumber)
            .Select(q => q.Id)
            .ToListAsync();

        return waiting.IndexOf(entryId) + 1;
    }

    private static QueueEntryResponseDto MapToDto(QueueEntry e, int position) => new()
    {
        Id                   = e.Id,
        TokenNumber          = e.TokenNumber,
        CustomerName         = e.CustomerName,
        PhoneNumber          = e.PhoneNumber,
        Status               = e.Status.ToString(),
        JoinedAt             = e.JoinedAt,
        Position             = position,
        EstimatedWaitMinutes = position * 25   // avg 10 min per customer
    };

    public async Task<object?> GetQueueSummaryAsync(int shopId)
    {
        var shop = await _db.Shops
            .Include(s => s.QueueEntries)
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null) return null;

        var today = DateTime.UtcNow.Date;

        var waitingCount = shop.QueueEntries.Count(q =>
            q.Status == QueueStatus.Waiting &&
            q.JoinedAt.Date == today);

        var servingToken = shop.QueueEntries
            .Where(q => q.Status == QueueStatus.Serving && q.JoinedAt.Date == today)
            .Select(q => (int?)q.TokenNumber)
            .FirstOrDefault();

        var avgServiceTime = shop.AvgServiceTime <= 0 ? 15 : shop.AvgServiceTime;

        return new
        {
            servingToken,
            waitingCount,
            estimatedWait = waitingCount * avgServiceTime,
            avgServiceTime
        };
    }

}
