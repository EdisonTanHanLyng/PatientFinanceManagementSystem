using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ReminderHub : Hub
{
    public async Task SendReminderUpdate(string message)
    {
        // Notify all clients
        await Clients.All.SendAsync("ReceiveReminderUpdate", message);
    }
}
