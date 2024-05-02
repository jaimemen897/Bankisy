using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TFG.Services.Hub;

[Authorize]
public class MyHub : Microsoft.AspNetCore.SignalR.Hub
{
    public static ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
    
    public override async Task OnConnectedAsync()
    {
        var username = Context.User.Identity.Name;
        var connectionId = Context.ConnectionId;

        _userConnections.TryAdd(username, connectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.User.Identity.Name;

        _userConnections.TryRemove(username, out _);

        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(string user, string message)
    {
        if (_userConnections.TryGetValue(user, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("SendMessage", user, message);
        }
    }
}