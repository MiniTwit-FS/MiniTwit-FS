using Microsoft.AspNetCore.SignalR;

namespace MiniTwitAPI.Hubs
{
    public class LogHub : Hub
    {
        public async Task SendLogUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveLogUpdate", message);
        }
    }
}