using Microsoft.AspNetCore.SignalR;

namespace TFG.Services.Hub;

public class NameUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.User.Identity.Name;
    }
}