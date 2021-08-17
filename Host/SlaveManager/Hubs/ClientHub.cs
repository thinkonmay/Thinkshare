using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SharedHost.Models;
using SlaveManager.Administration;
using SlaveManager.Models;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public interface IClientHub
    {
        Task ReportSlaveObtained(int slaveID);

        Task ReportNewSlaveAvailable(SlaveDeviceInformation device);

        Task ReportSessionDisconnected(int slaveID);

        Task ReportSessionReconnected(int slaveID);
    }



    public class ClientHub : Hub<IClientHub>
    { 

    }
}