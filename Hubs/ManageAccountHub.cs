using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
public class ManageAccountHub : Hub
{
    public async Task UpdateUserStatus(string userId) //Verify for user login and refresh manage acc page
    {
        await Clients.All.SendAsync("ReceiveManageAccUpdate", userId);
    }

    public async Task UserLoggedIn(string userId)
    {
        // Notify all clients that a user has logged in
        await Clients.All.SendAsync("UserLoggedIn", userId);
    }

    public async Task UserLoggedOut(string userId)
    {
        // Notify all clients that a user has logged out
        await Clients.All.SendAsync("UserLoggedOut", userId);
    }
    

}
