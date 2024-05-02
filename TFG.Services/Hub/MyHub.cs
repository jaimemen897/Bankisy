using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TFG.Services.Hub;

[Authorize]
public class MyHub : Microsoft.AspNetCore.SignalR.Hub
{
    public static ConcurrentDictionary<string, string> _userConnections = new();
    
    public override async Task OnConnectedAsync()
    {
        /*httpContextAccessor.HttpContext!*/
        var userId = Context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        var connectionId = Context.ConnectionId;

        _userConnections.TryAdd(userId, connectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        _userConnections.TryRemove(userId, out _);

        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(string user, string message)
    {
        if (_userConnections.TryGetValue(user, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("TransferReceived", user, message);
        }
    }
}