using Microsoft.AspNetCore.SignalR;

namespace TFG.Services.Hub;

public class MyHub : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}