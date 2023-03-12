using Microsoft.AspNetCore.SignalR;
using RestartApplicationPool.Share.Enums;

namespace RestartApplicationPool.SignalR.Hubs;

public class ChatHub : Hub
{
    public const string Template = "/chathub";
    public async Task SendMessage(ActionType actionType)
    {
        await Clients.All.SendAsync("ReceiveMessage", (int)actionType);
    }
    
    public async Task ReturnMessage(string message)
    {
        await Clients.All.SendAsync("ReturnMessage", message);
    }
}