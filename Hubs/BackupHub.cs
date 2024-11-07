using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PFMS_MI04.Hubs
{
    public class BackupHub : Hub
    {
        public async Task UpdateBackupList(string message)
        {
            await Clients.All.SendAsync("ReceiveBackupListUpdate", message);
        }
    }
}
