using Microsoft.AspNetCore.SignalR;

namespace CutBook.API.Hubs;

public class QueueHub : Hub
{
    // Customer ya barber ek shop ki "room" join karta hai
    public async Task JoinShopRoom(string shopId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"shop-{shopId}");
    }

    public async Task LeaveShopRoom(string shopId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shop-{shopId}");
    }
}
