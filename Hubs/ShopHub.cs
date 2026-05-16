using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CutBookApi.Hubs;

[Authorize]
public class ShopHub : Hub
{
    /// <summary>
    /// ShopOwner calls this to join their shop group.
    /// Customers call this with their user group to receive updates.
    /// </summary>
    public async Task JoinShopGroup(int shopId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");
    }

    public async Task LeaveShopGroup(int shopId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shop_{shopId}");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            // Every user auto-joins their personal group for direct notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{userId}");
        }
        await base.OnConnectedAsync();
    }
}
